// <copyright file="GuanoParser.WildlifeAcoustics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using Emu.Models.Notices;

    public static partial class GuanoParser
    {
        private const string SmartQuoteFixNotice = "Normalized smart quotes in `{0}` JSON.";

        private static GuanoEntry NormalizeWildlifeAcousticsEntry(GuanoEntry entry, List<Notice> notices)
        {
            // Some Wildlife Acoustics files appear to have GUANO values re-encoded with Unicode smart quotes
            // (U+201C “ / U+201D ”) instead of ASCII double quotes which is invalid JSON.
            // Observed in fixture WA_SMM/3.4_NormalAndCorrupt/SMM215_20231117_094400.wav, which
            // also carries a WA|Kaleidoscope|Version entry, suggesting Kaleidoscope re-encoded
            // the GUANO block and introduced the smart quotes.
            // This _could_ show up in JSON-looking values, not just
            // one field, so we normalize any such entry and record a notice for the affected key.
            // xref: https://github.com/QutEcoacoustics/emu/issues/439
            if (string.IsNullOrWhiteSpace(entry.Value) || !LooksLikeJson(entry.Value) || !ContainsSmartQuotes(entry.Value))
            {
                return entry;
            }

            notices.Add(new Warning(string.Format(SmartQuoteFixNotice, (string)entry.Key)));

            return entry with
            {
                Value = NormalizeSmartQuotes(entry.Value),
            };
        }

        private static bool LooksLikeJson(string value)
        {
            var trimmedValue = value.TrimStart();
            return trimmedValue.Length > 1 && (trimmedValue[0] == '{' || trimmedValue[0] == '[');
        }

        private static bool ContainsSmartQuotes(string value)
        {
            return value.IndexOf('\u201C') >= 0 || value.IndexOf('\u201D') >= 0;
        }

        private static string NormalizeSmartQuotes(string value)
        {
            return value.Replace('\u201C', '"').Replace('\u201D', '"');
        }
    }
}
