// <copyright file="FilenameParsingFixtureData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Emu.Serialization;

    public class FilenameParsingFixtureData : IEnumerable<object[]>
    {
        private const string FixtureFile = "FileNameParsingFixtures.csv";
        private readonly FilenameParsingFixtureModel[] filenameParsingFixtureModels;

        public FilenameParsingFixtureData()
        {
            var path = FixtureHelper.ResolvePath(FixtureFile);
            using var streamReader = FixtureHelper.RealFileSystem.File.OpenText(path);
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
}
