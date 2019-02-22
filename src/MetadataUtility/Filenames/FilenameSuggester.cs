// <copyright file="FilenameSuggester.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Filenames
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MetadataUtility.Models;
    using NodaTime.Text;

    /// <summary>
    /// Suggests new stable filenames for audio files.
    /// </summary>
    /// <remarks>
    /// The "one true" naming standard.
    /// </remarks>
    public class FilenameSuggester
    {
        // ReSharper disable StringLiteralTypo

        private static readonly OffsetDateTimePattern DatePattern = OffsetDateTimePattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss;FFFFFFo<Z+HHmm>");

        // ReSharper restore StringLiteralTypo

        private static readonly HashSet<char> InvalidChars = new HashSet<char>(Path.GetInvalidFileNameChars())
        {
            '\\', '/', ':', '[', ']', '{', '}', ';',
        };

        private static readonly char[] TrimChars = { '_', '-', '.' };

        private char replacementChar = '-';
        private char segmentSeparator = '_';

        /// <summary>
        /// Suggests a filename for a a recording.
        /// </summary>
        /// <remarks>
        /// The general format: <code>date_component1_component2_location.extension</code>.
        /// </remarks>
        /// <returns>The suggested filename.</returns>
        public MetadataSource<string> SuggestName(Recording recording, ParsedFilename filename, SuggestedNameOptions options)
        {
            if (recording.StartDate == null)
            {
                return default;
            }

            // Why 64? Just because most filenames will be smaller than that and we avoid the expensive
            // array size operation.
            var result = new StringBuilder(64);

            DatePattern.AppendFormat(recording.StartDate.Value, result);

            foreach (var segment in new[] { filename.Prefix, filename.Suffix })
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    continue;
                }

                result.Append(this.segmentSeparator);
                this.Clean(segment, result);
            }

            result.Append(filename.Extension);

            return result.ToString().SourcedFrom(Provenance.Filename);
        }

        private void Clean(string input, StringBuilder aggregate)
        {
            var chars = input.AsSpan().Trim(TrimChars);

            for (int index = 0; index < chars.Length; index++)
            {
                var c = chars[index];
                if (InvalidChars.Contains(c))
                {
                    aggregate.Append(this.replacementChar);
                }
                else if (c == ' ')
                {
                    // pascal case
                    if ((index + 1) < chars.Length)
                    {
                        var next = chars[index + 1];
                        if (next == this.replacementChar)
                        {
                            // do nothing (do not duplicate replacement char)
                        }
                        else if (char.IsLower(next))
                        {
                            aggregate.Append(char.ToUpperInvariant(next));

                            // skip the next char in the loop
                            index++;
                        }
                        else
                        {
                            aggregate.Append(this.replacementChar);
                        }
                    }

                    // otherwise, it's last char, so just don't add to result
                }
                else
                {
                    aggregate.Append(c);
                }
            }
        }

        /// <summary>
        /// Options that affect how the renamer works.
        /// </summary>
        public class SuggestedNameOptions
        {
        }
    }
}
