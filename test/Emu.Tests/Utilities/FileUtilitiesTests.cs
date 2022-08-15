// <copyright file="FileUtilitiesTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Tests.TestHelpers;
    using Emu.Utilities;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

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

            buffer.Fill(0);
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
    }
}
