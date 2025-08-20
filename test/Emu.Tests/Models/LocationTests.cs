// <copyright file="LocationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Models
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Emu.Models;
    using FluentAssertions;
    using LanguageExt;

    public class LocationTests
    {
        public static readonly Location Default = new()
        {
            LatitudePrecision = 0,
            LongitudePrecision = 0,
        };

        // split from theory data so xunit can serialize test names
        public static readonly Dictionary<string, Location> TestCases = new()
        {
            { "+00-025/", Default with { Latitude = 0, Longitude = -25, } },
            { "+46+002/", Default with { Latitude = 46, Longitude = 2, } },
            { "+48.52+002.20/", Default with { Latitude = 48.52, LatitudePrecision = 2, Longitude = 2.2, LongitudePrecision = 2 } },
            { "+48.8577+002.295/", Default with { Latitude = 48.8577, LatitudePrecision = 4, Longitude = 2.295, LongitudePrecision = 3 } },
            {
                "+27.5916+086.5640+8850CRSWGS_84/",
                Default with
                {
                    Latitude = 27.5916,
                    LatitudePrecision = 4,
                    Longitude = 86.5640,
                    LongitudePrecision = 4,
                    Altitude = 8850,
                    AltitudePrecision = 0,
                    CoordinateReferenceSystem = "WGS_84",
                }
            },
            { "+90+000/", Default with { Latitude = 90, Longitude = 0, } },
            { "+00-160/", Default with { Latitude = 0, Longitude = -160, } },
            {
                "-90+000+2800CRSWGS_84/",
                Default with
                {
                    Latitude = -90,
                    Longitude = 0,
                    Altitude = 2800,
                    AltitudePrecision = 0,
                    CoordinateReferenceSystem = "WGS_84",
                }
            },
            { "+38-097/", Default with { Latitude = 38, Longitude = -97, } },
            { "+40.75-074.00/", Default with { Latitude = 40.75, LatitudePrecision = 2, Longitude = -74, LongitudePrecision = 2 } },
            { "+40.6894-074.0447/", Default with { Latitude = 40.6894, LatitudePrecision = 4, Longitude = -74.0447, LongitudePrecision = 4 } },
            { string.Empty, null },
            { "+A+B/", null },
        };

        public static IEnumerable<object[]> Data => TestCases.Select(x => new object[] { x.Key });

        [Theory]
        [MemberData(nameof(Data))]
        public void LocationParsingWorks(string input)
        {
            var expected = TestCases[input];
            var success = Location.TryParse(input, out var actual);

            Assert.Equal(expected is not null, success);

            actual.Should().BeEquivalentTo(expected);
        }

        [SkippableTheory]
        [MemberData(nameof(Data))]
        public void LocationFormattingWorksForH(string expected)
        {
            var actual = TestCases[expected];

            Skip.If(actual is null);

            actual.ToString("H", CultureInfo.InvariantCulture).Should().Be(expected);
        }

        [SkippableTheory]
        [MemberData(nameof(Data))]
        public void LocationFormattingWorksForFilenames(string expected)
        {
            var actual = TestCases[expected];

            Skip.If(actual is null);

            actual.ToString("h", CultureInfo.InvariantCulture).Should().Be(expected.TrimEnd('/'));
        }

        [Fact]
        public void ItCanParseTruncatedValuesButFormatsThemWide()
        {
            // this is kinda a violation of the spec but it's more useful that we can parse malformed values
            var input = "+4.52-2.20/";
            var expected = "+04.52-002.20/";

            var success = Location.TryParse(input, out var actual);

            Assert.True(success);
            Assert.Equal(4.52, actual.Latitude);
            Assert.Equal(-2.2, actual.Longitude);
            Assert.Equal(expected, actual.ToString());
        }

        [Fact]
        public void ItCanParseTruncatedValuesButFormatsThemWide2()
        {
            // this is kinda a violation of the spec but it's more useful that we can parse malformed values
            var input = "-1+2/";
            var expected = "-01+002/";

            var success = Location.TryParse(input, out var actual);

            Assert.True(success);
            Assert.Equal(-1, actual.Latitude);
            Assert.Equal(2, actual.Longitude);
            Assert.Equal(expected, actual.ToString());
        }
    }
}
