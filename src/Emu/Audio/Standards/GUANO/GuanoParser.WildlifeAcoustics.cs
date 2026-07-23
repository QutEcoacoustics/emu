// <copyright file="GuanoParser.WildlifeAcoustics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using System;
    using Emu.Models.Notices;

    public static partial class GuanoParser
    {
        private const string SmartQuoteFixNotice = "Guano: Normalized smart quotes in `{0}` JSON.";
        private const string TimestampFixNotice = "Guano: Normalized non-spec timestamp in `{0}` from `{1}` to `{2}`.";

        private static string NormalizeWildlifeAcousticsEntry(GuanoKey key, string value, List<Notice> notices)
        {
            if (key.Vendor != Metadata.WildlifeAcoustics.Guano.Namespace)
            {
                return value;
            }

            // Some Wildlife Acoustics files appear to have GUANO values re-encoded with Unicode smart quotes
            // (U+201C “ / U+201D ”) instead of ASCII double quotes which is invalid JSON.
            // Observed in fixture WA_SMM/3.4_NormalAndCorrupt/SMM215_20231117_094400.wav, which
            // also carries a WA|Kaleidoscope|Version entry, suggesting Kaleidoscope re-encoded
            // the GUANO block and introduced the smart quotes.
            // This _could_ show up in JSON-looking values, not just
            // one field, so we normalize any such entry and record a notice for the affected key.
            // xref: https://github.com/QutEcoacoustics/emu/issues/439
            if (string.IsNullOrWhiteSpace(value) || !LooksLikeJson(value) || !ContainsSmartQuotes(value))
            {
                return value;
            }

            notices.Add(new Info(string.Format(SmartQuoteFixNotice, key.FullKey)));

            return NormalizeSmartQuotes(value);
        }

        private static string NormalizeWildlifeAcousticsTimestampEntry(GuanoKey key, string value, List<Notice> notices)
        {
            if (key != GuanoTimestampKey || string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            // Adapter for known non-spec Wildlife Acoustics timestamps using a space instead of 'T'
            // e.g. 2023-11-17 09:52:00+11:00
            if (value.Length > 10
                && value[10] == ' '
                && value[4] == '-'
                && value[7] == '-')
            {
                var normalized = value[..10] + "T" + value[11..];
                notices.Add(new Info(string.Format(TimestampFixNotice, key.FullKey, value, normalized)));
                return normalized;
            }

            return value;
        }

        private static bool LooksLikeJson(string value)
        {
            var trimmedValue = value.TrimStart();
            return trimmedValue.Length > 1 && (trimmedValue[0] == '{' || trimmedValue[0] == '[');
        }

        private static bool ContainsSmartQuotes(string value)
        {
            return value.Contains('\u201C') || value.Contains('\u201D');
        }

        private static string NormalizeSmartQuotes(string value)
        {
            return value.Replace('\u201C', '"').Replace('\u201D', '"');
        }
    }
}
