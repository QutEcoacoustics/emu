// <copyright file="FilenameGenerator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Filenames
{
    using System;
    using System.Collections;
    using System.IO.Abstractions;
    using System.Text.RegularExpressions;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Dates;
    using Emu.Models;
    using LanguageExt;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using NodaTime;
    using Rationals;
    using SmartFormat;
    using SmartFormat.Core.Extensions;
    using SmartFormat.Core.Formatting;
    using SmartFormat.Core.Parsing;
    using SmartFormat.Extensions;
    using Error = LanguageExt.Common.Error;

    public class FilenameGenerator
    {
        public const char Delimiter = '_';

#pragma warning disable IO0006 // Replace Path class with IFileSystem.Path for improved testability
        private static readonly System.Collections.Generic.HashSet<char> InvalidChars = new(Path.GetInvalidFileNameChars())
        {
            '\\',
            '/',
            ':',
            '[',
            ']',
            '{',
            '}',
            ';',
        };
#pragma warning restore IO0006 // Replace Path class with IFileSystem.Path for improved testability

        private static readonly char[] TrimChars = { '_', '-', ' ' };

        private static readonly string Replacement = string.Empty;

        // I tried generating this regex from the InvalidChars field but it turned out to be an
        // encoding nightmare. This is simpler.
        private static readonly Regex InvalidCharRegex = new(@"[\u0000-\u0019""<>|*?/\\:;[\]{}]+");

        private readonly IFileSystem fileSystem;
        private readonly ILogger<FilenameGenerator> logger;
        private readonly SmartFormatter smart;

        public FilenameGenerator(IFileSystem fileSystem, ILogger<FilenameGenerator> logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
            this.smart = new SmartFormatter()
                .AddExtensions(
                    new StringSource(),
                    new ListFormatter(),
                    new DictionarySource(),
                    new ValueTupleSource(),
                    new ReflectionSource(),
                    new DefaultSource(),
                    new KeyValuePairSource())
                .AddExtensions(new DefaultFormatter())
                .InsertExtension(0, new EmptyFormatter())
                .InsertExtension(1, new FileNameFormatter());
        }

        public string ReconstructAndNormalize(ParsedFilename filename)
        {
            var cleanedTokenizedName = this.NormalizeParsedTokens(filename.NameTokens);
            return this.smart.Format(cleanedTokenizedName, filename);
        }

        public Fin<string> Reconstruct(string tokenizedName, Recording recording)
        {
            try
            {
                return this.smart.Format(tokenizedName, recording);
            }
            catch (FormattingException fex) when (fex.Message.Contains("No source extension could handle the selector"))
            {
                this.logger.LogTrace("Error occurred while formatting", fex);
                return Error.New($"Unknown field `{fex?.ErrorItem?.RawText}` in the template `{fex?.ErrorItem?.BaseString}`.");
            }
        }

        public string NormalizeParsedTokens(Lst<FilenameToken> tokens)
        {
            // Need to join segments together with delimiters
            // need to clean segments on literals
            return tokens.Map(Normalize).Join(string.Empty);

            string Normalize(int index, FilenameToken current)
            {
                var previous = index > 0 ? tokens[index - 1] : null;
                var next = index < tokens.Count - 1 ? tokens[index + 1] : null;

                string segment;
                if (current is FilenameToken.Literal literal)
                {
                    var cleaned = this.CleanSegment(literal.Text);

                    segment = cleaned;
                }
                else
                {
                    segment = current.ToString();
                }

                if (string.IsNullOrEmpty(segment))
                {
                    return string.Empty;
                }

                // we don't add a delimiter if
                // - it's the first token
                var firstToken = previous is null;

                // - the token has told us it should be abutted compactly
                var compactToken = current is FilenameToken.Value { Compact: true };

                // - or it is suitable to delimit a segment anyway
                var suitableDelimiter = segment.StartsWith('.');

                if (!(firstToken || compactToken || suitableDelimiter))
                {
                    segment = Delimiter + segment;
                }

                return segment;
            }
        }

        public string CleanSegment(string segment)
        {
            if (segment is null)
            {
                return null;
            }

            // ok: this is pretty hacky but I don't like periods at the end
            // of metadata segments. Prefixed; ok: they're like extensions.
            // trailing? Doesn't make sense. So TrimEnd only for '.'
            return InvalidCharRegex
                .Replace(segment, Replacement)
                .Trim(TrimChars)
                .TrimEnd('.');
        }

        private class FileNameFormatter : IFormatter
        {
            public string Name { get; set; } = "file";

            public bool CanAutoDetect { get; set; } = true;

            public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
            {
                var format = formattingInfo.Format;
                var formatText = format?.ToString();
                var provider = formattingInfo.FormatDetails.Provider;

                var (can, result) = formattingInfo.CurrentValue switch
                {
                    OffsetDateTime o => (true, DateFormatting.FormatFileName(o)),
                    LocalDateTime l => (true, DateFormatting.FormatFileName(l)),
                    TimeSpan t => (true, t.TotalSeconds.ToString(formatText, provider)),
                    Location l => (true, l.ToString("h", provider)),
                    Rational r => (true, r.IsNaN ? r.ToString() : ((decimal)r).ToString(formatText ?? "F3", provider)),
                    _ => (false, null),
                };

                if (can)
                {
                    formattingInfo.Write(result!);
                }

                return can;
            }
        }

        private class EmptyFormatter : IFormatter
        {
            public string Name { get; set; } = "ifempty";

            public bool CanAutoDetect { get; set; } = true;

            public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
            {
                var formats = formattingInfo.Format?.Split('|');

                var empty = formattingInfo.CurrentValue switch
                {
                    null => true,
                    string s when s.Length == 0 => true,
                    IEnumerable e when e.GetEnumerator().MoveNext() => true,
                    _ => false,
                };

                if (formats is null)
                {
                    return false;
                }
                else if (empty)
                {
                    formattingInfo.Write(formattingInfo?.Format?.GetLiteralText() ?? string.Empty);
                    return true;
                }
                else
                {
                    if (formats.Count == 2)
                    {
                        formattingInfo.FormatAsChild(formats[1], formattingInfo.CurrentValue);
                    }
                    else
                    {
                        var selector = ((FormattingInfo)formattingInfo).Selector;
                        if (selector is null)
                        {
                            return false;
                        }

                        var newPlaceholder = "{" + selector.RawText + "}";
                        var newFormat = formattingInfo.FormatDetails.Formatter.Parser.ParseFormat(newPlaceholder);
                        formattingInfo.FormatAsChild(newFormat, formattingInfo.CurrentValue);
                    }

                    return true;
                }
            }
        }
    }
}
