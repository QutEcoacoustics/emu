// <copyright file="HashCalculator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.Threading.Tasks;
    using Emu.Models;
    using Emu.Utilities;

    public class HashCalculator : IMetadataOperation
    {
        private readonly FileUtilities fileUtilities;

        public HashCalculator(FileUtilities fileUtilities)
        {
            this.fileUtilities = fileUtilities;
        }

        public string Name => nameof(HashCalculator);

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            // as long as the file exists, we can calculate a hash.
            return ValueTask.FromResult(true);
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
