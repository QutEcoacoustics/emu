// <copyright file="Metadata.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Metadata
{
    using System.CommandLine.Invocation;
    using System.IO.Abstractions;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Cli;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Emu.Utilities;
    using Microsoft.Extensions.Logging;

    using static Emu.Cli.SpectreUtils;

    public class Metadata : EmuCommandHandler<Recording>
    {
        private readonly ILogger<Metadata> logger;
        private readonly IFileSystem fileSystem;
        private readonly FileMatcher fileMatcher;

        private readonly MetadataRegister extractorRegister;

        public Metadata(
            ILogger<Metadata> logger,
            IFileSystem fileSystem,
            FileMatcher fileMatcher,
            OutputRecordWriter writer,
            MetadataRegister register)
        {
            this.logger = logger;
            this.fileSystem = fileSystem;
            this.fileMatcher = fileMatcher;
            this.Writer = writer;
            this.extractorRegister = register;
        }

        public string[] Targets { get; set; }

        // public bool Save {get; set;}

        public override async Task<int> InvokeAsync(InvocationContext invocationContext)
        {
            var paths = this.fileMatcher.ExpandMatches(this.fileSystem.Directory.GetCurrentDirectory(), this.Targets);

            Dictionary<string, List<TargetInformation>> targetDirectories = new Dictionary<string, List<TargetInformation>>();

            // Group targets together according to their directories
            // This is done so that only one search for support files is done per directory
            foreach (var path in paths)
            {
                using var context = this.CreateContainer(path);

                string directory = context.FileSystem.Path.GetDirectoryName(context.Path);

                if (targetDirectories.ContainsKey(directory))
                {
                    targetDirectories[directory].Add(context);
                }
                else
                {
                    targetDirectories[directory] = new List<TargetInformation> { context };
                }
            }

            this.WriteHeader();

            // Extract recording information from each target
            foreach ((string directory, List<TargetInformation> targets) in targetDirectories)
            {
                SupportFile.FindSupportFiles(directory, targets, this.fileSystem);

                foreach (TargetInformation target in targets)
                {
                    Recording recording = new Recording
                    {
                        SourcePath = target.Path,
                    };

                    foreach (var extractor in this.extractorRegister.All)
                    {
                        if (await extractor.CanProcessAsync(target))
                        {
                            recording = await extractor.ProcessFileAsync(target, recording);
                        }
                    }

                    this.Write(recording);
                }
            }

            this.WriteFooter();

            return ExitCodes.Success;
        }

        public override string FormatCompact(Recording record)
        {
            var values = new string[]
            {
                record.SourcePath,
                record.MediaType,
                record.DurationSeconds.ToString(),
                record.SampleRateHertz.ToString(),
                record.Channels.ToString(),
                record.BitDepth.ToString(),
                record.BitsPerSecond.ToString(),
                record.Sensor.Name,
                record.Sensor.Firmware,
            };

            var formatted = string.Join("\t", values);

            return formatted;
        }

        public override object FormatRecord(Recording record)
        {
            return FormatList<Recording>(record);
        }

        private TargetInformation CreateContainer((string Base, string File) target)
        {
            return new TargetInformation(this.fileSystem)
            {
                Base = target.Base,
                Path = target.File,
            };
        }
    }
}
