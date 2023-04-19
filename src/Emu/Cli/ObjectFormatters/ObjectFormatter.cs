// <copyright file="ObjectFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli.ObjectFormatters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Emu.Dates;
    using LanguageExt;
    using NodaTime;
    using Spectre.Console;
    using Duration = NodaTime.Duration;

    public abstract class ObjectFormatter
    {
        public ObjectFormatter()
        {
        }

        [RequiresUnreferencedCode("Prints objects via their properties. If only domain objects are printed, then this is safe.")]
        public void Print(StringBuilder builder, object record, Options options = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(builder));
            var values = SimplifyObject(record);

            var (indent, except, keyPrefix) = options;
            foreach (var kvp in values)
            {
                var key = options.KeyPrefix + kvp.Key;
                var value = kvp.Value;

                if (except?.Invoke(key) ?? false)
                {
                    continue;
                }

                // expand type?
                var type = value?.GetType();

                // expand records unless they are iformattable - our heuristic that something is a small
                // single value is that it can be formatted.
                if (IsRecord(value))
                {
                    var objectOptions = this.StartObject(builder, key, value, type, in options);

                    // recurse!
                    this.Print(builder, value, objectOptions with { Depth = indent + 1 });

                    this.EndObject(builder, key, value, type, in options);
                }
                else if (IsList(value) is (true, var complex))
                {
                    IReadOnlyList<object> list = ((IEnumerable)value!).Cast<object>().ToList();
                    var innerBuilder = new StringBuilder();

                    var listOptions = this.StartList(innerBuilder, key, list, complex, options);

                    if (complex)
                    {
                        // recurse!
                        this.Print(innerBuilder, value, listOptions with { Depth = indent + 1 });
                    }
                    else
                    {
                        var line = ((IEnumerable)value!)
                            .Cast<object>()
                            .Select(x => this.StyleValue(x, key, this.FormatValue(x, key)))
                            .FormatInlineList(", ");
                        innerBuilder.Append(line);
                    }

                    this.EndList(innerBuilder, key, list, complex, options);

                    builder.Append(innerBuilder);
                }
                else
                {
                    // otherwise format value
                    var formatted = this.FormatValue(value, key);
                    var styledValue = this.StyleValue(value, key, formatted);
                    var styledKey = this.StyleKey(key, value);
                    this.Append(builder, styledKey, styledValue, options);
                }
            }
        }

        protected virtual string FormatValue(object value, string name)
        {
            return value switch
            {
                null => string.Empty,
                string s => s,

                Rationals.Rational r => ((decimal)r).ToString("G"),
                LocalDate d => DateFormatting.LocalDatePattern.Format(d),
                LocalTime t => DateFormatting.LocalTimePattern.Format(t),
                LocalDateTime l => DateFormatting.DatePatternISO8601.Format(l),
                OffsetDateTime o => DateFormatting.OffsetDateTimePattern.Format(o),
                Duration d => DateFormatting.DurationISO8601HoursTotal.Format(d),
                Offset o => DateFormatting.OffsetPattern.Format(o),
                Enum e => e.GetEnumMemberValueOrDefault(),
                Range r => r.Start.ToString() + ".." + r.End.ToString(),

                // recursive!
                IEither e => e.MatchUntyped(right => this.FormatValue(right, name), left => this.FormatValue(left, name)),

                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),

                object x => x.ToString(),
            };
        }

        protected abstract Options StartObject(StringBuilder builder, string key, object obj, Type type, in Options options);

        protected abstract void EndObject(StringBuilder builder, string key, object obj, Type type, in Options options);

        protected abstract Options StartList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options);

        protected abstract void EndList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options);

        protected abstract string StyleValue(object value, string key, string converted);

        protected abstract string StyleKey(string key, object value);

        protected abstract void Append(StringBuilder builder, string key, string value, in Options options);

        [RequiresUnreferencedCode("Calls System.ComponentModel.TypeDescriptor.GetProperties(Object)")]
        private static IEnumerable<KeyValuePair<string, object>> SimplifyObject(object record)
        {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            return record switch
            {
                IEnumerable<KeyValuePair<string, object>> alreadyGood => alreadyGood,
                IDictionary d => d.Keys.Cast<object>().Select(x => KeyValuePair.Create(x.ToString()!, d[x])),
                IEnumerable e when IsList(e) is (true, _) => e.Cast<object?>().Select((x, i) => KeyValuePair.Create(i.ToString()!, x)),
                object o => TypeDescriptor
                    .GetProperties(o)
                    .Cast<PropertyDescriptor>()
                    .Select(x => KeyValuePair.Create(x.Name, x.GetValue(record))),
            };
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        }

        private static bool IsRecord(object value) => value switch
        {
            null => false,
            IFormattable => false,
            IConvertible => false,
            object o when o.GetType().IsRecordClass() => true,
            _ => false,
        };

        private static (bool Is, bool Complex) IsList(object value) => value switch
        {
            string => (false, false),
            null => (false, false),
            IEither => (false, false),
            IEnumerable<KeyValuePair<string, object>> => (true, true),
            IEnumerable => (true, IsRecord((value as IReadOnlyCollection<object>)?.FirstOrDefault())),

            _ => (false, false),
        };

        public readonly record struct Options(int Depth = 0, Func<string, bool> Except = null, string KeyPrefix = "");
    }
}
