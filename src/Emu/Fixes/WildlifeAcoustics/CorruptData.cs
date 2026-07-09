// <copyright file="CorruptData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.WildlifeAcoustics
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Utilities;
    using Microsoft.Extensions.Logging;

    public class CorruptData(IFileSystem fileSystem, FileUtilities fileUtilities, ILogger<CorruptData> logger) : ICheckOperation
    {
        private static readonly byte[][] KnownMagicNumbers =
        [
            Wave.RiffMagicNumber,        // RIFF (WAV)
            Flac.FlacMagicNumber,        // fLaC
            "OggS"u8.ToArray(),          // Ogg Vorbis/Opus
            "ID3"u8.ToArray(),           // MP3 with ID3 tag
        ];

        private readonly IFileSystem fileSystem = fileSystem;
        private readonly FileUtilities fileUtilities = fileUtilities;
        private readonly ILogger<CorruptData> logger = logger;

        public static OperationInfo Metadata { get; } = new(
            WellKnownProblems.WildlifeAcoustics.CorruptData,
            Fixable: false,
            Safe: true,
            Automatic: false,
            typeof(CorruptData),
            Suffix: "corrupt");

        public async Task<CheckResult> CheckAffectedAsync(string file)
        {
            using var stream = this.fileSystem.File.OpenRead(file);

            if (stream.Length == 0)
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            // Check if file starts with any known audio format magic number.
            // This is more reliable than checking file extensions which can be wrong.
            Span<byte> header = stackalloc byte[Wave.MinimumRiffHeaderLength];
            var bytesRead = stream.Read(header);

            if (bytesRead >= 4 && StartsWithKnownMagic(header))
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            // File doesn't have RIFF header - check it's not all null bytes (that's OE005)
            stream.Position = 0;
            var allNull = await this.fileUtilities.CheckForContinuousValue(stream);

            if (allNull)
            {
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            // Check if the byte distribution is uniform - a strong indicator of corrupt random data
            stream.Position = 0;
            var uniformity = await this.fileUtilities.CheckForUniformDistribution(stream, sampleSize: 4096);

            this.logger.LogDebug("Uniformity check result: {uniformity}", uniformity);

            if (!uniformity.IsUniform)
            {
                // The file is still corrupt, but it has some structure, so it is not WA004.
                // A core tenant of our fixes is that are very specific. We don't want a Word
                // document to be flagged as WA004 just because it is not a valid WAV file.
                // So we only flag files that are completely unstructured.
                return new CheckResult(CheckStatus.Unaffected, Severity.None, string.Empty);
            }

            return new CheckResult(
                CheckStatus.Affected,
                Severity.Severe,
                "The file has no valid RIFF/WAVE header and contains only unstructured data. The data is not recoverable.");
        }

        public OperationInfo GetOperationInfo() => Metadata;

        private static bool StartsWithKnownMagic(ReadOnlySpan<byte> header)
        {
            foreach (var magic in KnownMagicNumbers)
            {
                if (header.Length >= magic.Length && header[..magic.Length].SequenceEqual(magic))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
