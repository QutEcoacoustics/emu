// <copyright file="DataSize0.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System.IO.Abstractions;
    using Emu.Audio.WAVE;
    using Range = Emu.Audio.RangeHelper.Range;

    public class DataSize0 : IncorrectDataSize
    {
        public const uint ErrorSize = 0;

        public DataSize0(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.DataSize0,
            Fixable: true,

            // we cannot detect any chunk after data, we just use remaining file size and assume there are no more chunks.
            Safe: false,
            Automatic: true,
            typeof(DataSize0));

        public override OperationInfo GetOperationInfo() => Metadata;

        protected override bool CheckIfDataSizeBad(Range dataRange, long streamLength)
        {
            return dataRange.Length == 0;
        }

        protected override bool CheckIfRiffSizeBad(Range riffRange, long streamLength)
        {
            // the first 8 bytes aren't counted in the total
            var expectedRiffLength = streamLength - Wave.MinimumRiffHeaderLength;

            return expectedRiffLength != riffRange.Length;
        }
    }
}
