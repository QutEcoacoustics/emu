// <copyright file="PrettyFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli.ObjectFormatters
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Emu.Models.Notices;
    using LanguageExt;
    using NodaTime;
    using Spectre.Console;

    using static Emu.Cli.SpectreUtils;
    using Duration = NodaTime.Duration;

    public class PrettyFormatter
        : ObjectFormatter
    {
        protected override void Append(StringBuilder builder, string key, string value, in Options options)
        {
            builder.AppendLine($"{Indent(options.Depth)}{key} = {value}");
        }

        protected override string StyleKey(string key, object value) => $"[yellow]{key}[/]";

        protected override string StyleValue(object value, string key, string converted)
        {
            var escaped = converted.EscapeMarkup();
            return value switch
            {
                null => string.Empty,
                string s when key.Contains("Path") => MarkupPath(escaped),
                string s when key.Contains("Directory") => MarkupPath(escaped),
                string s => s.EscapeMarkup(),

                Rationals.Rational r => MarkupNumber(escaped),
                LocalDate d => MarkupDate(escaped),
                LocalTime t => MarkupDate(escaped),
                LocalDateTime l => MarkupDate(escaped),
                OffsetDateTime o => MarkupDate(escaped),
                Duration d => MarkupDate(escaped),
                Offset o => MarkupDate(escaped),
                bool b => MarkupBool(b),
                Enum e => MarkupEnum(escaped),
                Range r => escaped.Split("..") is[var a, var b] ? $"{MarkupNumber(a)}..{MarkupNumber(b)}" : escaped,
                Info i => MarkupInfo(escaped),
                Warning w => MarkupWarning(escaped),
                Error e => MarkupError(escaped),

                // recursive!
                IEither e => e.MatchUntyped(right => this.StyleValue(right, key, escaped), left => this.StyleValue(left, key, escaped)),

                IFormattable f => MarkupNumber(escaped),

                _ => escaped,
            };
        }

        protected override Options StartList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options)
        {
            var styledKey = this.StyleKey(key, list);
            if (list.Count < 1 || !complex)
            {
                builder.Append($"{Indent(options.Depth)}{styledKey} = [[ ");
            }
            else
            {
                this.Append(builder, styledKey, "[[", options);
            }

            return options;
        }

        protected override void EndList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options)
        {
            if (list.Count < 1)
            {
                builder.AppendLine("]]");
            }
            else if (!complex)
            {
                builder.AppendLine(" ]]");
            }
            else
            {
                builder.AppendLine(Indent(options.Depth) + "]]");
            }
        }

        protected override Options StartObject(StringBuilder builder, string key, object obj, Type type, in Options options)
        {
            var styledKey = this.StyleKey(key, obj);
            this.Append(builder, styledKey, MarkupType(type?.Name), options);

            return options;
        }

        protected override void EndObject(StringBuilder builder, string key, object obj, Type type, in Options options)
        {
        }

        private static string Indent(int indent) => new(' ', indent * 2);
    }
}
