// <copyright file="FixtureHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Emu.Serialization;
    using Xunit;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    public static partial class FixtureHelper
    {
        public static IFileSystem RealFileSystem { get; } = new FileSystem();

        public static string ResolvePath(string name)
        {
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, name));

            if (!RealFileSystem.File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find name {name} at path {path}");
            }

            return path;
        }

        public static string ResolveDirectory(string name)
        {
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, name));

            if (!RealFileSystem.Directory.Exists(path))
            {
                throw new FileNotFoundException($"Could not find name {name} at path {path}");
            }

            return path;
        }

        public static string ResolveFirstDirectory(string name)
        {
            // be convention all of the paths in our fixtures CSV uses `/`
            var firstDirectory = name.Split('/').First();
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, firstDirectory));

            if (!RealFileSystem.Directory.Exists(path))
            {
                throw new FileNotFoundException($"Could not find directory {firstDirectory} at path {path}");
            }

            return path;
        }

        public class FilenameParsingFixtureData : IEnumerable<object[]>
        {
            private const string FixtureFile = "FileNameParsingFixtures.csv";
            private readonly FilenameParsingFixtureModel[] filenameParsingFixtureModels;

            public FilenameParsingFixtureData()
            {
                using var streamReader = RealFileSystem.File.OpenText(ResolvePath(FixtureFile));
                var serializer = new CsvSerializer();

                this.filenameParsingFixtureModels = serializer
                    .Deserialize<FilenameParsingFixtureModel>(streamReader)
                    .ToArray();
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
            private const string FixtureFile = "Fixtures.yaml";
            private static readonly Dictionary<string, FixtureModel> FixtureModels;

            static FixtureData()
            {
                using var streamReader = RealFileSystem.File.OpenText(ResolvePath(FixtureFile));

                var parser = new MergingParser(new Parser(streamReader));
                var deserializer = new DeserializerBuilder().Build();

                FixtureModels = deserializer
                    .Deserialize<FixtureModel[]>(parser)
                    .ToDictionary(f => f.Name);
            }

            public FixtureData()
            {
            }

            public static IReadOnlyCollection<FixtureModel> All => FixtureModels.Values;

            public FixtureModel this[string key] => FixtureModels[key];

            public static FixtureModel Get(string key) => FixtureModels[key];

            public IEnumerator<object[]> GetEnumerator()
            {
                IEnumerable<object[]> models = FixtureModels
                    .Select(x => new object[] { x.Value })
                    .ToArray();

                return models.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
