// <copyright file="WellKnownProblems.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Emu.Models;
    using LanguageExt;
    using static LanguageExt.Prelude;

    /// <summary>
    /// A collection of well-known problems that we can encounter with sensors.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1623:Property summary documentation should match accessors",
        Justification = "These are not ordinary properties")]
    public static class WellKnownProblems
    {
        public const string Group = "OE";

        private static readonly IReadOnlyDictionary<string, WellKnownProblem> Problems;

        static WellKnownProblems()
        {
            Problems = typeof(WellKnownProblems)
                            .GetNestedTypes()
                            .Prepend(typeof(WellKnownProblems))
                            .SelectMany(t => t.GetProperties(BindingFlags.Static | BindingFlags.Public))
                            .Where(p => p.PropertyType == typeof(WellKnownProblem))
                            .Select(p => p.GetValue(null))
                            .Cast<WellKnownProblem>()
                            .ToDictionary(wkp => wkp.Id, wkp => wkp);
        }

        /// <summary>
        /// Happens when no date can be found.
        /// </summary>
        /// <returns>An pre-filled <see cref="Error"/> instance.</returns>
        public static WellKnownProblem NoDateFound => new(
            "Missing Date",
            "A date could not be determined for this file",
            "001",
            Group,
            null);

        /// <summary>
        /// Happens when a date does not include any offset information.
        /// </summary>
        /// <returns>An pre-filled <see cref="WellKnownProblem"/> instance.</returns>
        public static WellKnownProblem AmbiguousDate => new(
            "Ambiguous Date",
            "A date was found for this file but we don't know how far way it is from UTC - it could be a dozen different times",
            "002",
            Group,
            null);

        public static WellKnownProblem InvalidDateStamp => new(
            "Invalid datestamp in file name",
            "The datestamp must match the standard defined by Open Ecoacoustics",
            "003",
            Group,
            null);

        public static FrontierLabsProblems FrontierLabs { get; } = new();

        public static bool TryLookup(string id, out WellKnownProblem problem)
        {
            return Problems.TryGetValue(id, out problem);
        }

        public static string PatchString(WellKnownProblem problem) => "EMU+" + problem.Id;

        public class FrontierLabsProblems
        {
            public const string Group = "FL";

            public WellKnownProblem MetadataDurationBug => new(
                "Metadata Duration Bug",
                "This file's duration is wrong.",
                "010",
                Group,
                null);

            public WellKnownProblem StubFile => new(
                "Stub file",
                "This file is only a stub, it has no data in it",
                "001",
                Group,
                null);
        }
    }
}
