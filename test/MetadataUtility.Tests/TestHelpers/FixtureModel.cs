// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.IO.Abstractions;
    using CsvHelper.Configuration.Attributes;
    using MetadataUtility.Audio;
    using MetadataUtility.Metadata;

    public enum ValidMetadata
    {
        No,
        Partial,
        Yes,
    }

    public class FixtureModel
    {
        public const string ShortFile = "Short error file";
        public const string MetadataDurationBug = "Metadata duration bug";
        public const string ZeroDbSamples = "Zero dB Samples";
        public const string NormalFile = "Normal file";

        private string fixturePath;

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Vendor { get; set; }

        public ValidMetadata ValidMetadata { get; set; }

        public string MimeType { get; set; }

        public decimal DurationSeconds { get; set; }

        public byte Channels { get; set; }

        public uint SampleRateHertz { get; set; }

        public uint BitsPerSecond { get; set; }

        public byte BitDepth { get; set; }

        public ulong TotalSamples { get; set; }

        // TODO: add other columns from the CSV here!

        public bool IsFlac => this.MimeType == Flac.Mime;

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

        public bool IsVendor(Vendor vendor) => Enum
                .GetName<Vendor>(vendor)
                .Equals(
                    this.Vendor.Replace(" ", string.Empty),
                    StringComparison.InvariantCultureIgnoreCase);

        public TargetInformation ToTargetInformation(IFileSystem fileSystem)
        {
            return new TargetInformation(fileSystem)
            {
                Path = this.AbsoluteFixturePath,
                Base = FixtureHelper.ResolveFirstDirectory(this.FixturePath),
            };
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
