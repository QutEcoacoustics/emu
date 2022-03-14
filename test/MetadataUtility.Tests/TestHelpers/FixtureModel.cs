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
    using MetadataUtility.Metadata.SupportFiles;
    using MetadataUtility.Models;
    using Rationals;

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

        public Rational DurationSeconds { get; set; }

        public byte Channels { get; set; }

        public uint SampleRateHertz { get; set; }

        public uint BitsPerSecond { get; set; }

        public byte BitDepth { get; set; }

        public ulong TotalSamples { get; set; }

        public string FrontierLabsLogFile { get; set; }

        public string[] Process { get; set; }

        public string SDFormatType { get; set; }

        public uint SDManufacturerID { get; set; }

        public string SDOEMID { get; set; }

        public string SDProductName { get; set; }

        public float SDProductRevision { get; set; }

        public uint SDSerialNumber { get; set; }

        public string SDManufactureDate { get; set; }

        public uint SDSpeed { get; set; }

        public uint SDCapacity { get; set; }

        public uint SDWrCurrentVmin { get; set; }

        public uint SDWrCurrentVmax { get; set; }

        public uint SDWriteB1Size { get; set; }

        public uint SDEraseB1Size { get; set; }

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

        public MemoryCard MemoryCard
        {
            get =>
                new MemoryCard() with
                {
                    FormatType = this.SDFormatType,
                    ManufacturerID = this.SDManufacturerID,
                    OEMID = this.SDOEMID,
                    ProductName = this.SDProductName,
                    ProductRevision = this.SDProductRevision,
                    SerialNumber = this.SDSerialNumber,
                    ManufactureDate = this.SDManufactureDate,
                    Speed = this.SDSpeed,
                    Capacity = this.SDCapacity,
                    WrCurrentVmin = this.SDWrCurrentVmin,
                    WrCurrentVmax = this.SDWrCurrentVmax,
                    WriteB1Size = this.SDWriteB1Size,
                    EraseB1Size = this.SDEraseB1Size,
                };
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
            TargetInformation ti = new TargetInformation(fileSystem) with
            {
                Path = this.AbsoluteFixturePath,
                Base = FixtureHelper.ResolveFirstDirectory(this.FixturePath),
            };

            foreach (Action<TargetInformation> func in SupportFile.SupportFileFinders)
            {
                func(ti);
            }

            return ti;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
