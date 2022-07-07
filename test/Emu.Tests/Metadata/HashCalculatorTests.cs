// <copyright file="HashCalculatorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Metadata
{
    using System.Linq;
    using Emu.Metadata;
    using Emu.Models;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class HashCalculatorTests : TestBase
    {
        private readonly HashCalculator subject;

        public HashCalculatorTests(ITestOutputHelper output)
            : base(output)
        {
            this.subject = new HashCalculator();
        }

        public Recording Recording => new();

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void CanProcessFilesWorks(FixtureModel model)
        {
            // we can process all files that exist
            var result = await this.subject.CanProcessAsync(model.ToTargetInformation(this.RealFileSystem));

            Assert.True(result);
        }

        [Theory]
        [ClassData(typeof(FixtureHelper.FixtureData))]
        public async void ProcessFilesWorks(FixtureModel model)
        {
            Recording expectedRecording = model.Record;

            var recording = await this.subject.ProcessFileAsync(
                model.ToTargetInformation(this.RealFileSystem),
                this.Recording);

            recording.CalculatedChecksum.Should().Be(expectedRecording.CalculatedChecksum);
        }
    }
}
