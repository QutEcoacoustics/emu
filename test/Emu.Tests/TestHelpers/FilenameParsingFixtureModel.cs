// <copyright file="FilenameParsingFixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using NodaTime;

    public class FilenameParsingFixtureModel
    {
        public string Filename { get; set; }

        public LocalDateTime? ExpectedDateTime { get; set; }

        public Offset? ExpectedTzOffset { get; set; }

        public double? ExpectedLatitude { get; set; }

        public double? ExpectedLongitude { get; set; }

        public string Extension { get; set; }

        public string TokenizedName { get; set; }

        public string NormalizedName { get; set; }

        public override string ToString()
        {
            return this.Filename;
        }
    }
}
