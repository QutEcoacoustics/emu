// <copyright file="GuanoParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using System.Text;
    using Emu.Audio.WAVE;
    using Emu.Models.Notices;
    using LanguageExt;
    using Error = LanguageExt.Common.Error;

    public static partial class GuanoParser
    {
        public const char NamespaceSeparator = '|';
        public const string GuanoNamespace = "GUANO";

        public static readonly byte[] GuanoChunkId = "guan"u8.ToArray();
        private const string GuanoNewLineEscape = @"\n";
        private const string GuanoNewLine = "\n";
        private const char EntrySeparator = '\n';

        private static readonly GuanoKey GuanoVersionKey = GuanoKey.Parse(GuanoNamespace + "|" + "Version");

        public static Fin<bool> HasGuanoChunk(Stream stream)
        {
            var chunk = GetGuanoChunk(stream);
            return chunk.Match(_ => true, _ => false);
        }

        public static Fin<RangeHelper.Range> GetGuanoChunk(Stream stream)
        {
            var riffChunk = Wave.FindRiffChunk(stream);
            var waveChunk = riffChunk.Bind(r => Wave.FindWaveChunk(stream, r));
            var guanoChunk = waveChunk
                .Bind(w => Wave.ScanForChunks(stream, w, GuanoChunkId, false))
                .Map(x => x.First());

            if (guanoChunk.IsFail)
            {
                return (Error)guanoChunk;
            }

            return guanoChunk;
        }

        public static Fin<(GuanoBlock Guano, List<Notice> Notices)> ReadGuanoBlock(Stream stream)
        {
            var chunk = GetGuanoChunk(stream);
            if (chunk.IsFail)
            {
                return (Error)chunk;
            }

            var bytes = RangeHelper.ReadRange(stream, (RangeHelper.Range)chunk);
            return ParseGuano(bytes);
        }

        public static Fin<(GuanoBlock Guano, List<Notice> Notices)> ParseGuano(ReadOnlySpan<byte> bytes)
        {
            var notices = new List<Notice>();
            var allFields = new System.Collections.Generic.HashSet<GuanoKey>();
            var entries = new Dictionary<GuanoKey, string>();
            string version = null;

            var text = Encoding.UTF8.GetString(bytes).Trim('\0', ' ', '\t', '\r', '\n');
            GuanoKey? firstKey = null;

            foreach (var rawLine in text.Split(EntrySeparator))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var separatorIndex = line.IndexOf(':');
                if (separatorIndex < 0)
                {
                    notices.Add(new Warning($"Ignoring malformed GUANO line (no key-value separator): `{line}`"));
                    continue;
                }

                var key = line[..separatorIndex].Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    notices.Add(new Warning("Ignoring malformed GUANO line with empty key."));
                    continue;
                }

                var parsedKey = GuanoKey.Parse(key);

                // The GUANO spec encodes newlines inside values as the literal two-character sequence
                // `\n`, so we decode that here before handing the value to downstream parsers.
                var value = line[(separatorIndex + 1)..].Trim().Replace(GuanoNewLineEscape, GuanoNewLine);
                firstKey ??= parsedKey;

                if (allFields.Contains(parsedKey))
                {
                    notices.Add(new Warning($"Ignoring duplicated GUANO key `{parsedKey.FullKey}`."));
                    continue;
                }

                allFields.Add(parsedKey);
                var normalizedValue = NormalizeWildlifeAcousticsEntry(parsedKey, value, notices);

                entries[parsedKey] = normalizedValue;

                if (parsedKey == GuanoVersionKey)
                {
                    version = normalizedValue;
                }
            }

            if (firstKey is not null && firstKey.Value != GuanoVersionKey)
            {
                notices.Add(new Warning($"Expected first GUANO key to be `{GuanoVersionKey.FullKey}`, found `{firstKey.Value.FullKey}`."));
            }

            if (version is null)
            {
                notices.Add(new Warning($"Missing required GUANO key `{GuanoVersionKey.FullKey}`."));
            }

            return (
                new GuanoBlock
                {
                    GuanoVersion = version,
                    Entries = entries,
                },
                notices);
        }
    }
}
