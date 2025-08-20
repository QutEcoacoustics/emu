// <copyright file="FileUtilitiesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using System.IO;
    using System.Numerics;
    using System.Threading.Tasks;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Utilities.DryRun;

    public class FileUtilitiesTests : TestBase
    {
        private readonly MemoryStream testStream;
        private readonly FileUtilities fileUtilities;

        public FileUtilitiesTests(ITestOutputHelper output)
            : base(output, true)
        {
            var stream = new MemoryStream();

            Span<byte> buffer = new byte[1024];

            buffer.Fill(1);

            stream.Write(buffer);

            buffer.Clear();
            stream.Write(buffer);

            buffer.Fill(2);
            stream.Write(buffer);
            stream.Flush();

            this.testStream = stream;

            this.fileUtilities = new FileUtilities(
                this.BuildLogger<FileUtilities>(),
                this.CurrentFileSystem);
        }

        [Fact]
        public async Task CheckForContinousValueWorks_False()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream);
            actual.Should().BeFalse();
        }

        [Fact]
        public async Task CheckForContinousValueWorks_True()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream);
            actual.Should().BeFalse();
        }

        [Fact]
        public async Task CheckForContinousValueWorks_Subset_1()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream, 0, 1024, new Vector<byte>(1));
            actual.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForContinousValueWorks_Subset_2()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream, 1024, 1024, new Vector<byte>(0));
            actual.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForContinousValueWorks_AcrossBoundary()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream, 1000, 200, new Vector<byte>(0));
            actual.Should().BeFalse();
        }

        [Fact]
        public async Task CheckForContinousValueWorks_WithLengthLongerThanStream()
        {
            var actual = await this.fileUtilities.CheckForContinuousValue(this.testStream, 2048, 99999, new Vector<byte>(2));
            actual.Should().BeTrue();
        }

        [Fact]
        public async Task CheckForContinuousValue_RemainderBytesLessThanVectorSize()
        {
            var stream = new MemoryStream();

            // a not nicely divideable chunk size
            var buffer = new byte[32 + 11];

            Array.Fill(buffer, (byte)99);

            stream.Write(buffer);

            var actual = await this.fileUtilities.CheckForContinuousValue(
                stream,
                target: new Vector<byte>(99));

            actual.Should().BeTrue();

            stream.Position = 34;

            // any other random value
            stream.Write("!"u8);

            actual = await this.fileUtilities.CheckForContinuousValue(
                stream,
                target: new Vector<byte>(99));

            actual.Should().BeFalse();
        }

        [Fact]
        public void CanTruncate()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();

            var newLength = this.fileUtilities.Truncate(this.testStream, 1024, dryRun(false));

            newLength.Should().Be(1024);
        }

        [Fact]
        public void TruncateDoesNothingOnADryRun()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();

            var newLength = this.fileUtilities.Truncate(this.testStream, 3072, dryRun(true));

            newLength.Should().Be(1024 * 3);
        }

        [Fact]
        public void TruncateCannotExtendAFile()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                this.fileUtilities.Truncate(this.testStream, 6000, dryRun(true)));

            ex.Message.Should().Contain("Cannot truncate a file when new length (6000) is longer than current length (3072)");
        }

        [Fact]
        public async Task CanTruncateSplitAsync()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();
            using var dest = new MemoryStream();

            await this.fileUtilities.TruncateSplitAsync(this.testStream, dest, 1024, dryRun(false));

            var buffer = new byte[1024];

            this.testStream.Position = 0;
            this.testStream.Length.Should().Be(1024);
            await this.testStream.ReadAsync(buffer);
            buffer.Should().AllBeEquivalentTo(1);

            dest.Position = 0;
            dest.Length.Should().Be(2048);
            await dest.ReadAsync(buffer);
            buffer.Should().AllBeEquivalentTo(0);
            await dest.ReadAsync(buffer);
            buffer.Should().AllBeEquivalentTo(2);
        }

        [Fact]
        public async Task TruncateSplitAsyncDoesNothingOnADryRun()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();
            using var dest = new MemoryStream();

            await this.fileUtilities.TruncateSplitAsync(this.testStream, dest, 1024, dryRun(true));

            this.testStream.Length.Should().Be(3072);
            dest.Length.Should().Be(0);
        }

        [Fact]
        public async Task TruncateSplitAsyncCannotExtendAFile()
        {
            var dryRun = this.ServiceProvider.GetRequiredService<DryRunFactory>();
            using var dest = new MemoryStream();

            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                this.fileUtilities.TruncateSplitAsync(this.testStream, dest, 6000, dryRun(true)));

            ex.Message.Should().Contain("Cannot truncate a file when new length (6000) is longer than current length (3072)");
        }
    }
}
