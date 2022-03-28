// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
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
        public const string SM4BatNormal1 = "SM4 Bat Normal 1";

        private string fixturePath;

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Vendor { get; set; }

        public ValidMetadata ValidMetadata { get; set; }

        public string MimeType { get; set; }

        public Rational DurationSeconds { get; set; }

        public ushort Channels { get; set; }

        public uint SampleRateHertz { get; set; }

        public uint BitsPerSecond { get; set; }

        public byte BitDepth { get; set; }

        public ulong TotalSamples { get; set; }

        public string FrontierLabsLogFile { get; set; }

        public string[] Process { get; set; }

        public MemoryCard MemoryCard { get; set; }

        public Sensor Sensor { get; set; }

        public ulong? FileLengthBytes { get; set; }

        public ushort? BlockAlign { get; set; }

        // TODO: add other columns from the CSV here!

        public bool IsFlac => this.MimeType == Flac.Mime;

        public bool IsWave => this.MimeType == Wave.Mime;

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
            TargetInformation ti = new TargetInformation(fileSystem) with
            {
                Path = this.AbsoluteFixturePath,
                Base = FixtureHelper.ResolveFirstDirectory(this.FixturePath),
            };

            SupportFile.FindSupportFiles(fileSystem.Path.GetDirectoryName(ti.Path), new List<TargetInformation> { ti }, fileSystem);

            return ti;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
