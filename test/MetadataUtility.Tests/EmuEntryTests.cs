// <copyright file="EmuEntryTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;

    public class EmuEntryTests : IClassFixture<FixtureHelper.FixtureData>
    {
        private readonly FixtureHelper.FixtureData data;

        public EmuEntryTests(FixtureHelper.FixtureData data)
        {
            this.data = data;
        }

        [Fact]
        public async void EmuWorks()
        {
            var testFile = this.data[FixtureModel.ShortFile];

            var result = await EmuEntry.Main(
                new[]
                {
                    testFile.AbsoluteFixturePath,
                });

            Assert.Equal(0, result);
        }
    }
}
