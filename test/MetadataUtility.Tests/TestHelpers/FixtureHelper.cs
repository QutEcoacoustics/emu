// <copyright file="FixtureHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MetadataUtility.Serialization;

    public static class FixtureHelper
    {
        public static string ResolvePath(string name)
        {
            var path = Path.Combine(Helpers.FixturesRoot, name);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find name {name} at path {path}");
            }

            return path;
        }

        public class FilenameParsingFixtureData : IEnumerable<object[]>
        {
            private const string FixtureFile = "FilenameParsigFixtures.csv";
            private readonly FilenameParsingFixtureModel[] filenameParsingFixtureModels;

            public FilenameParsingFixtureData()
            {
                using (var streamReader = File.OpenText(ResolvePath(FixtureFile)))
                {
                    var serializer = new CsvSerializer();
                    this.filenameParsingFixtureModels = serializer
                        .Deserialize<FilenameParsingFixtureModel>(streamReader)
                        .ToArray();
                }
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                IEnumerable<object[]> models = this.filenameParsingFixtureModels
                    .Select(x => new object[] { x });

                return models.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class FixtureData : IEnumerable<object[]>
        {
            private const string FixtureFile = "Fixtures.csv";
            private readonly Dictionary<string, FixtureModel> fixtureModels;

            public FixtureData()
            {
                using (var streamReader = File.OpenText(ResolvePath(FixtureFile)))
                {
                    var serializer = new CsvSerializer();

                    this.fixtureModels = serializer
                        .Deserialize<FixtureModel>(streamReader)
                        .ToDictionary(f => f.Name);
                }
            }

            public FixtureModel this[string key] => this.fixtureModels[key];

            public IEnumerator<object[]> GetEnumerator()
            {
                IEnumerable<object[]> models = this.fixtureModels
                    .Select(x => new object[] { x })
                    .ToArray();

                return models.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public class TestTempDir : IDisposable
        {
            public TestTempDir()
            {
                var basename = Path.GetRandomFileName();

                this.TempDir = Path.Join(Directory.GetCurrentDirectory(), basename);

                Directory.CreateDirectory(this.TempDir);
            }

            ~TestTempDir()
            {
                this.Dispose();
            }

            public string TempDir { get; }

            public void Dispose()
            {
                Directory.Delete(this.TempDir, recursive: true);
            }
        }
    }
}
