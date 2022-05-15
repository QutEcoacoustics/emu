// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Reflection;
    using CsvHelper.Configuration.Attributes;
    using MetadataUtility.Audio;
    using MetadataUtility.Metadata;
    using MetadataUtility.Metadata.SupportFiles;
    using MetadataUtility.Models;

    public enum ValidMetadata
    {
        No,
        Partial,
        Yes,
    }

    public class FixtureModel
    {
        public const string ShortFile = "Short Error File";
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

        private string fixturePath;

        public Recording Record { get; set; }

        public string Name { get; set; }

        public string Vendor { get; set; }

        public ValidMetadata ValidMetadata { get; set; }

        public string MimeType { get; set; }

        public string[] OverwriteProperties { get; set; }

        public Dictionary<string, FixtureModel> Process { get; set; }

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

        /// <summary>
        /// Overwrite certain values in the recording.
        /// Used for edge cases with some extractor.
        /// </summary>
        /// <param name="overwriteRecording">The FixtureModel containing the values to overwrite.</param>
        public void ApplyOverwrites(FixtureModel overwriteRecording)
        {
            foreach (string overwrite in overwriteRecording.OverwriteProperties)
            {
                var nestedProperties = overwrite.Split(".");
                object currentObject = this.Record, overwriteValue = overwriteRecording.Record;
                PropertyInfo property = currentObject.GetType().GetProperty(nestedProperties[0]);

                // Locate the property if nested
                for (int i = 1; i < nestedProperties.Length; i++)
                {
                    currentObject = property.GetValue(currentObject);
                    overwriteValue = property.GetValue(overwriteValue);

                    property = currentObject.GetType().GetProperty(nestedProperties[i]);
                }

                overwriteValue = property.GetValue(overwriteValue);

                property.SetValue(currentObject, overwriteValue);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
