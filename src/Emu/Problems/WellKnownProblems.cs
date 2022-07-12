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

        public static bool TryLookup(string id, out WellKnownProblem problem)
        {
            return Problems.TryGetValue(id, out problem);
        }

        public static string PatchString(WellKnownProblem problem) => "EMU+" + problem.Id;

        private static string MakeUrl(string group, string code)
        {
            var longGroup = group switch
            {
                OpenEcoacousticsProblems.Group => "open_ecoacoustics",
                FrontierLabsProblems.Group => "frontier_labs",
                _ => throw new ArgumentException($"Unknown group {group}"),
            };

            return $"https://github.com/ecoacoustics/known-problems/blob/main/{longGroup}/{group}{code}.md";
        }

        public class OpenEcoacousticsProblems
        {
            public const string Group = "OE";

            /// <summary>
            /// Happens when no date can be found.
            /// </summary>
            /// <returns>An pre-filled <see cref="Error"/> instance.</returns>
            public static WellKnownProblem NoDateFound => new(
                "Missing Date",
                "A date could not be determined for this file",
                "001",
                Group,
                MakeUrl(Group, "001"));

            /// <summary>
            /// Happens when a date does not include any offset information.
            /// </summary>
            /// <returns>An pre-filled <see cref="WellKnownProblem"/> instance.</returns>
            public static WellKnownProblem AmbiguousDate => new(
                "Ambiguous Date",
                "A date was found for this file but we don't know how far way it is from UTC - it could be a dozen different times",
                "002",
                Group,
                MakeUrl(Group, "002"));

            public static WellKnownProblem InvalidDateStamp => new(
                "Invalid datestamp in file name",
                "The datestamp must match the standard defined by Open Ecoacoustics",
                "003",
                Group,
                MakeUrl(Group, "003"));
        }

        public class FrontierLabsProblems
        {
            public const string Group = "FL";

            public static WellKnownProblem PreAllocatedHeader => new("Stub file", "This file is only a stub, it has no data in it", "001", Group, MakeUrl(Group, "001"));

            public static WellKnownProblem EmptyBlocksOfData => new("Empty blocks of data", "Empty data block found in file ", "002", Group, MakeUrl(Group, "002"));

            public static WellKnownProblem CorruptFullSizeFiles => new("Corrupt full size files", "The WAVE header's data chunk size was not correctly encoded", "003", Group, MakeUrl(Group, "003"));

            public static WellKnownProblem SquareBrackets => new("Square brackets in filename", "Square brackets are not allowed in filenames", "004", Group, MakeUrl(Group, "004"));

            public static WellKnownProblem IncorrectSubChunk2 => new("Incorrect SubChunk2 size", "The WAVE header's data chunk is the size of the file, not the chunk", "005", Group, MakeUrl(Group, "005"));

            public static WellKnownProblem ScheduleNamesRandom => new("Random schedule names", "The schedule names in the log files ae incorrect", "006", Group, MakeUrl(Group, "006"));

            public static WellKnownProblem IncorrectCID => new("Incorrect CID", "The SD Card ID in the CID is incorrectly encoded", "007", Group, MakeUrl(Group, "007"));

            public static WellKnownProblem InvalidDateStampSpaceZero => new("Invalid datestamp (space)", "There is a space character where a zero character should be", "008", Group, MakeUrl(Group, "008"));

            public static WellKnownProblem MetadataDurationBug => new("Metadata Duration Bug", "This file's duration is wrong.", "010", Group, MakeUrl(Group, "010"));

            public static WellKnownProblem PartialDataFiles => new("Partial file named data", "The data file is incomplete?", "011", Group, MakeUrl(Group, "011"));
        }
    }
}
