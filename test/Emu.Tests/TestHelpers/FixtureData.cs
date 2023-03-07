// <copyright file="FixtureData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    public class FixtureData : IEnumerable<object[]>
    {
        public const string FixtureFile = "Fixtures.yaml";
        private readonly Dictionary<string, FixtureModel> fixtureModels;

        public FixtureData()
        {
            // I did have this in a static constructor for performance reasons
            // but XUnit caches static access between test runs which makes debugging
            // tests really painful - a clean and build is needed between runs where
            // only fixture data has changed.
            var path = FixtureHelper.ResolvePath(FixtureFile);
            using var streamReader = FixtureHelper.RealFileSystem.File.OpenText(path);

            var parser = new MergingParser(new Parser(streamReader));
            var deserializer = new DeserializerBuilder().Build();

            this.fixtureModels = deserializer
                .Deserialize<FixtureModel[]>(parser)
                .ToDictionary(f => f.Name);
        }

        public IReadOnlyCollection<FixtureModel> All => this.fixtureModels.Values;

        public FixtureModel this[string key] => this.fixtureModels[key];

        public IEnumerator<object[]> GetEnumerator()
        {
            IEnumerable<object[]> models = this.fixtureModels
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
