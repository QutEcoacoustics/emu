// <copyright file="DataSizeOffBy44.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes.FrontierLabs
{
    using System.IO.Abstractions;
    using Emu.Audio.WAVE;
    using Range = Emu.Audio.RangeHelper.Range;

    public class DataSizeOffBy44 : IncorrectDataSize
    {
        public const int ErrorAmount = 44;

        public DataSizeOffBy44(IFileSystem fileSystem)
            : base(fileSystem)
        {
        }

        public static OperationInfo Metadata => new(
            WellKnownProblems.FrontierLabsProblems.IncorrectSubChunk2,
            Fixable: true,

            // we cannot detect any chunk after data, we just use remaining file size and assume there are no more chunks.
            Safe: false,
            Automatic: true,
            typeof(DataSizeOffBy44));

        public override OperationInfo GetOperationInfo() => Metadata;

        public Range ModifyDataRange(Range range)
        {
            return range with
            {
                End = range.End - ErrorAmount,
            };
        }

        protected override bool CheckIfDataSizeBad(Range dataRange, long streamLength)
        {
            // we're affected if the RIFF header is 44 bytes off

            return (dataRange.End - streamLength) == ErrorAmount;
        }

        protected override bool CheckIfRiffSizeBad(Range riffRange, long streamLength)
        {
            // the first 8 bytes aren't counted in the total
            var expectedRiffLength = streamLength - Wave.MinimumRiffHeaderLength;

            // or if the data header is 44 bytes off.
            return (riffRange.Length - expectedRiffLength) == ErrorAmount;
        }
    }
}
