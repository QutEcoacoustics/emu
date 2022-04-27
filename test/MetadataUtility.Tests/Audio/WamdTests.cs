// <copyright file="WamdTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Audio
{
    using System.Linq;
    using FluentAssertions;
    using MetadataUtility.Audio;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class WamdTests : TestBase, IClassFixture<FixtureHelper.FixtureData>
    {
        public WamdTests(ITestOutputHelper output, FixtureHelper.FixtureData data)
            : base(output)
        {
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void HasVersion1WamdChunkTest(FixtureModel model)
        {
            bool hasWamd = Wamd.HasVersion1WamdChunk(model.ToTargetInformation(this.RealFileSystem).FileStream).IfFail(false);

            hasWamd.Should().Be(model.CanProcess.Contains("WamdExtractor"));
        }
    }
}
