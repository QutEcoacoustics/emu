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
        /// About 11mm.
        /// https://en.wikipedia.org/wiki/Decimal_degrees.
        /// </summary>
        public const double Wgs84Epsilon = 0.0000001;

        public static readonly Action Nop = () => { };
    }
}
