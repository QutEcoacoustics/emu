// <copyright file="HashCalculator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Emu.Models;
    using Emu.Utilities;

    public class HashCalculator : IMetadataOperation
    {
        private readonly FileUtilities fileUtilities;
        private readonly EmuGlobalOptions options;

        public HashCalculator(FileUtilities fileUtilities, EmuGlobalOptions options)
        {
            this.fileUtilities = fileUtilities;
            this.options = options;
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            // Calculate the hash unless the user specifies otherwise
            return ValueTask.FromResult(!this.options.NoChecksum);
        }

        public async ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var result = await this.fileUtilities.CalculateChecksumSha256(information.FileStream);

            return recording with
            {
                CalculatedChecksum = result,
            };
        }
    }
}
