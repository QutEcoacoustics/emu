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

    public static class GuanoParser
    {
        public static readonly byte[] GuanoChunkId = "guan"u8.ToArray();
        private const string GuanoVersionKey = "GUANO|Version";

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

        public static Fin<(GuanoBlock Guano, List<Notice> Notices)> ExtractMetadata(Stream stream)
        {
            var chunk = GetGuanoChunk(stream);
            if (chunk.IsFail)
            {
                return (Error)chunk;
            }

            var bytes = RangeHelper.ReadRange(stream, (RangeHelper.Range)chunk);
            var notices = new List<Notice>();
            var allFields = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            var entries = new List<GuanoEntry>();
            string version = null;

            var text = Encoding.UTF8.GetString(bytes).Trim('\0', ' ', '\t', '\r', '\n');
            var firstKey = default(string);

            foreach (var rawLine in text.Split('\n'))
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

                var value = line[(separatorIndex + 1)..].Trim();
                firstKey ??= key;

                if (allFields.Contains(key))
                {
                    notices.Add(new Warning($"Ignoring duplicated GUANO key `{key}`."));
                    continue;
                }

                allFields.Add(key);

                var entry = ParseEntry(key, value);
                entries.Add(entry);

                if (string.Equals(key, GuanoVersionKey, StringComparison.Ordinal))
                {
                    version = entry.Value;
                }
            }

            if (firstKey is not null && !string.Equals(firstKey, GuanoVersionKey, StringComparison.Ordinal))
            {
                notices.Add(new Warning($"Expected first GUANO key to be `{GuanoVersionKey}`, found `{firstKey}`."));
            }

            if (!allFields.Contains(GuanoVersionKey))
            {
                notices.Add(new Warning($"Missing required GUANO key `{GuanoVersionKey}`."));
            }

            return (
                new GuanoBlock
                {
                    Version = version,
                    Entries = entries,
                },
                notices);
        }

        private static GuanoEntry ParseEntry(string key, string value)
        {
            var split = key.Split('|');

            if (split.Length > 1)
            {
                return new GuanoEntry
                {
                    Namespaces = split[..^1],
                    Field = split[^1],
                    Value = value,
                };
            }

            return new GuanoEntry
            {
                Namespaces = Array.Empty<string>(),
                Field = key,
                Value = value,
            };
        }
    }
}
