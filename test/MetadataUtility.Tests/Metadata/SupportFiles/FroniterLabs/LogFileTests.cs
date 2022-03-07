// <copyright file="LogFileTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Metadata.SupportFiles
{
    using System.Linq;
    using FluentAssertions;
    using LanguageExt;
    using MetadataUtility.Metadata.SupportFiles.FrontierLabs;
    using MetadataUtility.Tests.TestHelpers;
    using Xunit;
    using Xunit.Abstractions;

    public class LogFileTests : TestBase
    {
        public LogFileTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public void ReadTotalSamplesTest(FixtureModel model)
        {
            if (model.Process.Contains("LogFileExtractor"))
            {
                Fin<bool> hasLogFile = LogFile.FileExists(model.ToTargetInformation(this.RealFileSystem));
                Assert.True(hasLogFile.IsSucc);
                ((bool)hasLogFile).Should().Be(true);
            }
        }
    }
}
