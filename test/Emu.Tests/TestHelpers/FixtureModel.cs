// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Reflection;
    using CsvHelper.Configuration.Attributes;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles;
    using Emu.Models;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public enum ValidMetadata
    {
        No,
        Partial,
        Yes,
    }

    public class FixtureModel : IXunitSerializable
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
        public const string TwoLogFiles1 = "Two Log Files 1";
        public const string ShortFile = "Short File";
        public const string WaveWithCues = "Generic WAVE with cue chunk";
        public const string WaveWithCuesAndLabels = "Generic WAVE with cue and label chunks";

        public const string NoDataHeader = "SM3 No Data";
        public const string NoDataHeader2 = "SM3 No Data 2";

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

        public MockFileData ToMockFileData()
        {
            var bytes = FixtureHelper.RealFileSystem.File.ReadAllBytes(this.AbsoluteFixturePath);
            return new MockFileData(bytes);
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

        public void Deserialize(IXunitSerializationInfo info)
        {
            var text = info.GetValue<string>("Value");
            JsonConvert.PopulateObject(text, this, Helpers.JsonSettings);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            var json = JsonConvert.SerializeObject(this, Helpers.JsonSettings);
            info.AddValue("Value", json);
        }
    }
}
