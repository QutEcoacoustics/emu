// <copyright file="HashCalculator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Emu.Models;

    public class HashCalculator : IMetadataOperation
    {
        private static readonly HashAlgorithmName Name = HashAlgorithmName.SHA256;

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            // as long as the file exists, we can calculate a hash.
            return ValueTask.FromResult(true);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            using var hasher = SHA256.Create();

            ArgumentNullException.ThrowIfNull(hasher, nameof(hasher));

            // using a separate file stream here to avoid conflicting with other readers
            // using var stream = information.FileSystem.File.Open(
            //     information.Path,
            //     FileMode.Open,
            //     FileAccess.Read,
            //     FileShare.Read);

            var result = await hasher.ComputeHashAsync(information.FileStream);

            return recording with
            {
                CalculatedChecksum = new Checksum()
                {
                    Type = Name.ToString(),
                    Value = result.ToHexString(),
                },
            };
        }
    }
}
