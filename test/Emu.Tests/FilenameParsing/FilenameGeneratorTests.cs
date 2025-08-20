// <copyright file="FilenameGeneratorTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.FilenameParsing
{
    using System;
    using Emu.Filenames;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using NodaTime;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;
    using static Emu.Filenames.FilenameParser;
    using static Emu.Tests.TestHelpers.Helpers;

    public class FilenameGeneratorTests : TestBase
    {
        private readonly FilenameGenerator generator;

        public FilenameGeneratorTests(ITestOutputHelper output)
            : base(output)
        {
            this.generator = this.ServiceProvider.GetRequiredService<FilenameGenerator>();
        }

        [Theory]
        [ClassData(typeof(FilenameParsingFixtureData))]
        public void CanNormalizeTheFileName(FilenameParsingFixtureModel test)
        {
            var parsed = this.FilenameParser.Parse(test.Filename);

            var actual = this.generator.ReconstructAndNormalize(parsed);

            actual.Should().Be(test.NormalizedName);
        }
    }
}
