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

    public static class FixtureHelper
    {
        public static string ResolveFixture(string name)
        {
            return name;
        }

        public class FilenameParsingFixtureData : IEnumerable<object[]>
        {
            private const string FixtureFile = "FilenameParsingFixtures.csv";

            public IEnumerator<object[]> GetEnumerator()
            {
                using (var file = File.OpenText(ResolveFixture(FixtureFile)))
                using (var deserailizer = new CsvHelper.CsvReader(file))
                {
                    return deserailizer.GetRecords<FilenameParsingFixtureModel>().Select(x => new object[] { x }).GetEnumerator();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
