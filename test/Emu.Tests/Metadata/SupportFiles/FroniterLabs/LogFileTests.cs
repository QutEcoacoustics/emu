// <copyright file="LogFileTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata.SupportFiles
{
    using Emu.Metadata;
    using Emu.Metadata.SupportFiles.FrontierLabs;
    using Emu.Tests.TestHelpers;
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
        public void HasLogFileTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FrontierLabsLogFileExtractor))
            {
                TargetInformation ti = model.ToTargetInformation(this.RealFileSystem);

                Assert.True(ti.TargetSupportFiles.ContainsKey(LogFile.LogFileKey));
            }
        }
    }
}
