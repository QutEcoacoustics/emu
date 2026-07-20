// <copyright file="FixtureDataGuanoCatalogTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Emu.Audio.Standards.GUANO;
    using FluentAssertions;
    using Xunit;

    public class FixtureDataGuanoCatalogTests : IClassFixture<FixtureData>
    {
        private readonly FixtureData data;

        public FixtureDataGuanoCatalogTests(FixtureData data)
        {
            this.data = data;
        }

        [Fact]
        public void HasGuanoMatchesGuanoDetector()
        {
            var mismatches = new List<string>();

            foreach (var fixture in this.data.All.Where(x => x.IsWave && !x.IsZippedFixture))
            {
                using var stream = FixtureHelper.RealFileSystem.File.OpenRead(fixture.AbsoluteFixturePath);
                var detected = GuanoParser.HasGuanoChunk(stream).IfFail(false);

                if (fixture.HasGuano != detected)
                {
                    mismatches.Add($"{fixture.Name}: declared {fixture.HasGuano}, detected {detected}");
                }
            }

            mismatches.Should().BeEmpty("every WAVE fixture should explicitly declare whether it contains GUANO metadata");
        }
    }
}
