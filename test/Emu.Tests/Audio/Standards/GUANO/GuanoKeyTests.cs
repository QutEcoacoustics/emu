// <copyright file="GuanoKeyTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Standards.GUANO
{
    using System.Collections.Generic;
    using Emu.Audio.Standards.GUANO;
    using FluentAssertions;
    using Xunit;

    public class GuanoKeyTests
    {
        [Fact]
        public void GuanoKeyIsStructurallyEqualToEquivalentKey()
        {
            var left = new GuanoKey("WA", "Song Meter", "Audio settings");
            var right = new GuanoKey("WA", "Song Meter", "Audio settings");

            left.Should().Be(right);
            left.GetHashCode().Should().Be(right.GetHashCode());
        }

        [Fact]
        public void GuanoKeyCanBeUsedAsDictionaryKey()
        {
            var key = new GuanoKey("WA", "Song Meter", "Audio settings");
            var equivalentKey = new GuanoKey("WA", "Song Meter", "Audio settings");

            var dictionary = new Dictionary<GuanoKey, string>
            {
                [key] = "[{\"gain\":18}]",
            };

            dictionary.TryGetValue(equivalentKey, out var value).Should().BeTrue();
            value.Should().Be("[{\"gain\":18}]");
        }

        [Fact]
        public void GuanoKeyToStringReturnsFullKey()
        {
            var key = new GuanoKey("WA", "Song Meter", "Audio settings");

            key.ToString().Should().Be("WA|Song Meter|Audio settings");
        }
    }
}
