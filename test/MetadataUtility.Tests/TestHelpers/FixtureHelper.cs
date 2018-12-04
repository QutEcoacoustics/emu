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
                IEnumerable<object[]> models;
                using (var streamReader = File.OpenText(ResolveFixture(FixtureFile)))
                {
                    var serializer = new CsvSerializer();
                    models = serializer
                        .Deserialize<FilenameParsingFixtureModel>(streamReader)
                        .Select(x => new object[] { x })
                        .ToArray();
                }

                return models.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
