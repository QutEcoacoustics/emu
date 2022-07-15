// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Reflection;
    using CsvHelper.Configuration.Attributes;
    using Emu.Audio;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Xunit.Abstractions;

    public enum ValidMetadata
    {
        No,
        Partial,
        Yes,
    }

    public class FixtureModel
    {
        public const string PreAllocatedHeader = "Short Error File";
        public const string PreAllocatedHeader2 = "Preallocated header 153";
        public const string ArtificialZeroes = "Artificial Zeroes";
        public const string MetadataDurationBug = "Metadata Duration Bug";
        public const string ZeroDbSamples = "Zero dB Samples";
        public const string NormalFile = "Normal File";
        public const string SM4BatNormal1 = "SM4 Bat Normal 1";
        public const string FilenameExtractor = "FilenameExtractor";
        public const string FlacHeaderExtractor = "FlacHeaderExtractor";
        public const string FlacCommentExtractor = "FlacCommentExtractor";
        public const string FrontierLabsLogFileExtractor = "FrontierLabsLogFileExtractor";
        public const string WamdExtractor = "WamdExtractor";
        public const string FLCommentAndLogExtractor = "FLCommentAndLogExtractor";
        public const string SpaceInDateStamp = "Space in date stamp";
        public const string IncorrectDataSize = "Incorrect data size";

        private string fixturePath;

        public Recording Record { get; set; }

        public string Name { get; set; }

        public string Vendor { get; set; }

        public ValidMetadata ValidMetadata { get; set; }

        public string MimeType { get; set; }

        public Dictionary<string, Recording> Process { get; set; }

        public bool IsFlac => this.MimeType == Flac.Mime;

        public bool IsWave => this.MimeType == Wave.Mime;

        public ushort? BlockAlign { get; set; }

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

        public IFileInfo ToFileInfo(IFileSystem fileSystem)
        {
             return fileSystem.FileInfo.FromFileName(this.AbsoluteFixturePath);
        }

        public bool ShouldProcess(string processKey, out Recording recording)
        {
            recording = null;

            var result = this.Process.ContainsKey(processKey);

            if (result)
            {
                recording = this.Process[processKey] ?? this.Record;
            }

            return result;
        }

        public override string ToString()
        {
            return this.Name + " (" + this.FixturePath + ")";
        }
    }
}
