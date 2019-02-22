// <copyright file="WellKnownProblems.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataUtility.Models;

    /// <summary>
    /// A collection of well-known problems that we can encounter with sensors.
    /// </summary>
    public static class WellKnownProblems
    {
        /// <summary>
        /// Happens when no date can be found.
        /// </summary>
        /// <returns>An pre-filled <see cref="Error"/> instance.</returns>
        public static Error NoDateFound()
        {
            return new Error()
            {
                Code = "0000001",
                Message = "A date could not be determined for this file",
                Title = "Missing Date",
            };
        }

        /// <summary>
        /// Happens when a date does not include any offset information.
        /// </summary>
        /// <returns>An pre-filled <see cref="Error"/> instance.</returns>
        public static Error AmbiguousDate()
        {
            return new Error()
            {
                Code = "0000002",
                Message = "A date was found for this file but we don't know how far way it is from UTC - it could be a dozen different times",
                Title = "Ambiguous Date",
            };
        }
    }
}
