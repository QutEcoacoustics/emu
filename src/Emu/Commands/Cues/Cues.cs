// <copyright file="Cues.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Cues
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Cli;
    using Emu.Metadata;
    using Emu.Utilities;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using Rationals;
    using Spectre.Console;
    using static Emu.Cli.SpectreUtils;

    public class Cues : EmuCommandHandler<CueResult>
    {
        private readonly ILogger<Cues> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;

        public Cues(
            ILogger<Cues> logger,
            IFileSystem fileSystem,
            FileMatcher fileMatcher,
            OutputRecordWriter writer)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.Writer = writer;
        }

        public string[] Targets { get; set; }

        public bool Export { get; set; }

        public override async Task<int> InvokeAsync(InvocationContext context)
        {
            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            this.WriteHeader();

            // Extract recording information from each target
            foreach (var (@base, file) in paths)
            {
                using var target = new TargetInformation(this.fileSystem, @base, file);

                if (target.IsPcmWaveFile() && !target.IsPreallocatedHeader())
                {
                    var results = Parse(target);

                    foreach (var result in results)
                    {
                        this.Write(result);
                    }

                    if (this.Export)
                    {
                        this.logger.LogDebug("Writing cue file");
                        await this.WriteCueFileAsync(target, results);
                    }
                    else
                    {
                        this.logger.LogDebug("Not writing cue file");
                    }
                }
                else
                {
                    this.logger.LogWarning("Not a WAVE file: {path}", target.Path);
                }
            }

            this.WriteFooter();

            return ExitCodes.Success;
        }

        public override string FormatCompact(CueResult record)
        {
            var values = new string[]
            {
                record.File,
                ((decimal)record.Position).ToString("F6"),
                JoinAllCueStrings(record.Cue),
            };

            var formatted = string.Join("\t", values);

            return formatted;
        }

        public override object FormatRecord(CueResult record)
        {
            var f = record;
            StringBuilder builder = new();

            builder.AppendFormat(
                "File {0}[{1}]: ".EscapeMarkup(),
                MarkupPath(f.File),
                MarkupNumber(((decimal)record.Position).ToString("F6")));
            builder.Append(JoinAllCueStrings(record.Cue));

            return builder.ToString();
        }

        private static string JoinAllCueStrings(Cue cue)
        {
            var joined = string
                    .Join(' ', cue.Label, cue.Note, cue.Text)
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .Trim();

            return string.IsNullOrWhiteSpace(joined) ? "<no label>" : joined;
        }

        private static IEnumerable<CueResult> Parse(TargetInformation target)
        {
            var stream = target.FileStream;

            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var formatChunk = waveChunk.Bind(w => Wave.FindFormatChunk(stream, w));
            var dataChunk = waveChunk.Bind(w => Wave.FindDataChunk(stream, w));
            var formatSpan = RangeHelper.ReadRange(stream, formatChunk.ThrowIfFail());

            var bitsPerSample = Wave.GetBitsPerSample(formatSpan);
            var sampleRate = Wave.GetSampleRate(formatSpan);
            var channels = Wave.GetChannels(formatSpan);

            var samples = dataChunk.Map(d => Wave.GetTotalSamples(d, channels, bitsPerSample));
            var duration = samples.Map(s => new Rational((uint)samples, (uint)sampleRate));

            var cuePoints = waveChunk.Bind(w => Wave.FindAndParseCuePoints(stream, w));

            if (cuePoints.IsFail)
            {
                yield break;
            }

            foreach (var cuePoint in cuePoints.ThrowIfFail())
            {
                Fin<Rational> position = samples
                    .Map(s => new Rational(cuePoint.SamplePosition, s))
                    .Map(fraction => fraction * duration.IfFail(Rational.NaN));

                yield return new CueResult(target.Path, position.IfFail(Rational.NaN), cuePoint);
            }
        }

        private async Task WriteCueFileAsync(TargetInformation target, IEnumerable<CueResult> results)
        {
            var cueFile = target.FileSystem.FileInfo.New(target.Path + ".cue.txt");
            this.logger.LogDebug("Writing Cue file to {path}", cueFile.FullName);

            StringBuilder builder = new();
            foreach (var result in results)
            {
                builder.Append(((decimal)result.Position).ToString("F6"));
                builder.Append('\t');

                var label = JoinAllCueStrings(result.Cue);

                builder.Append(label);
                builder.Append(Environment.NewLine);
            }

            await target.FileSystem.File.WriteAllTextAsync(cueFile.FullName, builder.ToString());

            this.WriteMessage("Cues saved to file " + MarkupPath(cueFile.FullName));
        }
    }

    public record CueResult(string File, Rational Position, Cue Cue);
}
