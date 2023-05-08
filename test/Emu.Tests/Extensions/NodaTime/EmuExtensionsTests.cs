// <copyright file="EmuExtensionsTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Extensions.NodaTime
{
    using global::NodaTime;
    using global::NodaTime.Text;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;

    public class EmuExtensionsTests
    {
        [Theory]
        [InlineData("Z", true)]
        [InlineData("+00:00", true)]
        [InlineData("-01:00", true)]
        [InlineData("+01:00", true)]
        [InlineData("+18:00", true)]
        [InlineData("-18:00", true)]
        [InlineData("+01:30", false)]
        [InlineData("-04:45", false)]
        [InlineData("+12:33:44", false)]
        public void IsWholeHourOffsetWorks(string text, bool expected)
        {
            var offset = OffsetPattern.GeneralInvariantWithZ.Parse(text).Value;

            var actual = offset.IsWholeHourOffset();
            Assert.Equal(expected, actual);
        }
    }
}
