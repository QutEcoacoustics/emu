// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Linq;
    using CsvHelper.Configuration.Attributes;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.FixtureGenerator;
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

    [GenerateFixtureHelpers(FixtureData.FixtureFile)]
    public partial class FixtureModel : IXunitSerializable
    {
        public const string FilenameExtractor = "FilenameExtractor";
        public const string FlacHeaderExtractor = "FlacHeaderExtractor";
        public const string FlacCommentExtractor = "FlacCommentExtractor";
        public const string FrontierLabsLogFileExtractor = "FrontierLabsLogFileExtractor";
        public const string WamdExtractor = "WamdExtractor";
        public const string FLCommentAndLogExtractor = "FLCommentAndLogExtractor";

        // constants for fixture names are generated automatically. See GenerateFixtureHelpersAttribute.cs

        private string fixturePath;
        private Dictionary<string, Recording> process;

        public Recording Record { get; set; }

        public string Name { get; set; }

        public string Make { get; set; }

        public ValidMetadata ValidMetadata { get; set; }

        public string MimeType { get; set; }

        public Dictionary<string, Recording> Process
        {
            get => this.process;
            set => this.process = value is null ? new() : value;
        }

        public string[] Problems { get; set; }

        public bool IsFlac => this.MimeType == Flac.Mime;

        public bool IsWave => this.MimeType == Wave.Mime;

        public string FixturePath
        {
            get => this.fixturePath;

            set
            {
                var sanitized = value?.Replace('\\', '/');
                this.fixturePath = sanitized;
                this.AbsoluteFixturePath = FixtureHelper.ResolvePath(sanitized);
            }
        }

        [Ignore]
        public string AbsoluteFixturePath { get; set; }

        [Ignore]
#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
        public string AbsoluteFixtureDirectory => Path.GetDirectoryName(this.AbsoluteFixturePath);
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability

        public string Notes { get; set; }

        public string EscapedAbsoluteFixturePath => this.AbsoluteFixturePath.Replace("\\", "\\\\");

        public bool IsMake(Vendor vendor) => Enum
                .GetName(vendor)
                .Equals(
                    this.Make.Replace(" ", string.Empty),
                    StringComparison.InvariantCultureIgnoreCase);

        public TargetInformation ToTargetInformation(IFileSystem fileSystem)
        {
            TargetInformation ti = new TargetInformation(
                fileSystem,
                FixtureHelper.ResolveFirstDirectory(this.FixturePath),
                this.AbsoluteFixturePath);

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

        public bool IsAffectedByProblem(WellKnownProblem problem)
        {
            return this.Problems.Contains(problem.Id);
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
