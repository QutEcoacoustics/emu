// <copyright file="WellKnownProblems.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataUtility.Models;

    public static class WellKnownProblems
    {
        public static Error NoDateFound()
        {
            return new Error()
            {
                Code = "0000001",
                Message = "A date could not be determined for this file",
                Title = "Missing Date",
            };
        }

        public static Error AmbiguousDate()
        {
            return new Error()
            {
                Code = "0000002",
                Message = "An unambiguous date could not be determined for this file",
                Title = "Ambiguous Date",
            };
        }
    }
}
