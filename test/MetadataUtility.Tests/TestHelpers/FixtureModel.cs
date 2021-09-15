// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using CsvHelper.Configuration.Attributes;

    public class FixtureModel
    {
        public const string ShortFile = "Short error file";
        public const string MetadataDurationBug = "Metadata duration bug";
        public const string ZeroDbSamples = "Zero dB Samples";
        public const string NormalFile = "Normal file";

        private string fixturePath;

        public string Name { get; set; }

        public string FixturePath
        {
            get => this.fixturePath;

            set
            {
                this.fixturePath = value;
                this.AbsoluteFixturePath = FixtureHelper.ResolvePath(value);
            }
        }

        [Ignore]
        public string AbsoluteFixturePath { get; set; }

        public string Notes { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
