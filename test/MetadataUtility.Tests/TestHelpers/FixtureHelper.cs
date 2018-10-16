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
    using System.Text;
    using MetadataUtility.Serialization;
    using Xunit;

    public static class FixtureHelper
    {
        public const string SolutionRoot = "../../../../..";
        public const string FixturesRoot = "test/Fixtures";

        public static string ResolveFixture(string name)
        {
            return Path.Combine(SolutionRoot, FixturesRoot, name);
        }

        public class FilenameParsingFixtureData : IEnumerable<object[]>
        {
            private const string FixtureFile = "FilenameParsingFixtures.csv";

            public IEnumerator<object[]> GetEnumerator()
            {
                IEnumerable<FilenameParsingFixtureModel> models;
                using (var file = File.OpenText(ResolveFixture(FixtureFile)))
                {
                    var deserailizer = new CsvSerializer();

                    models = deserailizer.Deserialize<FilenameParsingFixtureModel>(file).ToArray();
                }

                return models.Select(x => new object[] { x }).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
