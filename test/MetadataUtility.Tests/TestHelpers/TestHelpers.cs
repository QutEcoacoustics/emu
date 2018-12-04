// <copyright file="TestHelpers.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class TestHelpers
    {
        /// <summary>
        /// About 111.32mm at the equator
        /// https://en.wikipedia.org/wiki/Decimal_degrees.
        /// </summary>
        public const int Wgs84Epsilon = 6;

        public static readonly Action Nop = () => { };
    }
}
