// <copyright file="LogFileTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata.SupportFiles
{
    using System.Linq;
    using Emu.Metadata.SupportFiles.FrontierLabs;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class LogFileTests : TestBase, IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public LogFileTests(ITestOutputHelper output, FixtureData data)
            : base(output, realFileSystem: true)
        {
            this.data = data;
        }

        [Theory]
        [ClassData(typeof(FixtureData))]
        public void HasLogFileTest(FixtureModel model)
        {
            if (model.Process.ContainsKey(FixtureModel.FrontierLabsLogFileExtractor))
            {
                var target = model.ToTargetInformation(this.CurrentFileSystem);

                Assert.True(target.TargetSupportFiles.ContainsKey(LogFile.LogFileKey));
            }
        }

        [Fact]
        public void CanDealWith300Oddities()
        {
            var model = this.data[FixtureModel.NormalFile300];

            // support files are added automatically with out helper
            var target = model.ToTargetInformation(this.CurrentFileSystem);

            LogFile logFile = (LogFile)target.TargetSupportFiles[LogFile.LogFileKey];

            var record = logFile.MemoryCardLogs.First();

            record.TimeStamp.Should().Be(new LocalDateTime(2022, 10, 02, 07, 53, 56));
            var memoryCard = record.Data;

            memoryCard.Should().BeEquivalentTo(new MemoryCard()
            {
                FormatType = "exFAT",
                ManufacturerID = 3,
                OEMID = "SD",
                ProductName = "ACLCF",
                ProductRevision = 8.0f,
                SerialNumber = 15435216,
                ManufactureDate = "2017-03",
                Speed = 50_000_000,
                Capacity = 124_868_608_000,
                WrCurrentVmin = 5,
                WrCurrentVmax = 5,
                WriteBlSize = 512,
                EraseBlSize = 65536,
            });
        }
    }
}
