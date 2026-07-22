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
    using Emu.Dates;
    using Emu.Models.Notices;
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

            if (record == null)
            {
                return;
            }

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

                if (TryGetDictionaryEntries(value, out var dictionaryEntries))
                {
                    var dictionaryOptions = this.StartDictionary(builder, key, dictionaryEntries, in options);

                    foreach (var entry in dictionaryEntries)
                    {
                        var formattedKey = this.FormatValue(entry.Key, key);
                        var outputKey = dictionaryOptions.KeyPrefix + formattedKey;
                        var styledKey = this.StyleDictionaryKey(outputKey, entry.Key, key);
                        var formattedValue = this.FormatValue(entry.Value, outputKey);
                        var styledValue = this.StyleValue(entry.Value, outputKey, formattedValue);
                        this.Append(builder, styledKey, styledValue, dictionaryOptions);
                    }

                    this.EndDictionary(builder, key, dictionaryEntries, in options);
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

                Rationals.Rational r => r.IsNaN ? r.ToString() : ((decimal)r).ToString("G"),
                LocalDate d => DateFormatting.LocalDatePattern.Format(d),
                LocalTime t => DateFormatting.LocalTimePattern.Format(t),
                LocalDateTime l => DateFormatting.DatePatternISO8601.Format(l),
                OffsetDateTime o => DateFormatting.OffsetDateTimePattern.Format(o),
                Duration d => DateFormatting.DurationISO8601HoursTotal.Format(d),
                Offset o => DateFormatting.OffsetPattern.Format(o),
                Enum e => e.GetEnumMemberValueOrDefault(),
                Range r => r.Start.ToString() + ".." + r.End.ToString(),
                Notice notice => notice.ToString("G", CultureInfo.InvariantCulture),

                // recursive!
                IEither e => e.MatchUntyped(right => this.FormatValue(right, name), left => this.FormatValue(left, name)),

                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),

                object x => x.ToString(),
            };
        }

        protected abstract Options StartObject(StringBuilder builder, string key, object obj, Type type, in Options options);

        protected abstract void EndObject(StringBuilder builder, string key, object obj, Type type, in Options options);

        protected abstract Options StartDictionary(StringBuilder builder, string key, IReadOnlyList<KeyValuePair<object, object>> dictionary, in Options options);

        protected abstract void EndDictionary(StringBuilder builder, string key, IReadOnlyList<KeyValuePair<object, object>> dictionary, in Options options);

        protected abstract Options StartList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options);

        protected abstract void EndList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options);

        protected abstract string StyleValue(object value, string key, string converted);

        protected abstract string StyleKey(string key, object value);

        protected virtual string StyleDictionaryKey(string key, object dictionaryKey, string dictionaryName) => this.StyleKey(key, dictionaryKey);

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
                    .Where(x => x.IsBrowsable)
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

            // I think this was casting to IReadOnlyCollection so we wouldn't treat things like strings as collections?
            // But we filter for string above, and
            // it omits too many other collection types. Trialing IEnumerable
            //IEnumerable => (true, IsRecord((value as IReadOnlyCollection<object>)?.FirstOrDefault())),
            IEnumerable => (true, IsRecord((value as IEnumerable<object>)?.FirstOrDefault())),

            _ => (false, false),
        };

        private static bool TryGetDictionaryEntries(object value, out IReadOnlyList<KeyValuePair<object, object>> entries)
        {
            switch (value)
            {
                case null:
                    entries = Array.Empty<KeyValuePair<object, object>>();
                    return false;
                case IDictionary dictionary:
                    entries = dictionary
                        .Cast<object>()
                        .Where(x => TryReadDictionaryItem(x, out _, out _))
                        .Select(x =>
                        {
                            _ = TryReadDictionaryItem(x, out var key, out var dictionaryValue);
                            return new KeyValuePair<object, object>(key, dictionaryValue);
                        })
                        .ToList();
                    return true;
                case IEnumerable enumerable:
                    return TryReadDictionaryEnumerable(enumerable, out entries);
                default:
                    entries = Array.Empty<KeyValuePair<object, object>>();
                    return false;
            }
        }

        private static bool TryReadDictionaryEnumerable(IEnumerable enumerable, out IReadOnlyList<KeyValuePair<object, object>> entries)
        {
            var list = new List<KeyValuePair<object, object>>();
            var hasAny = false;

            foreach (var item in enumerable)
            {
                hasAny = true;
                if (!TryReadDictionaryItem(item, out var key, out var dictionaryValue))
                {
                    entries = Array.Empty<KeyValuePair<object, object>>();
                    return false;
                }

                list.Add(new KeyValuePair<object, object>(key, dictionaryValue));
            }

            entries = list;
            return hasAny;
        }

        private static bool TryReadDictionaryItem(object value, out object key, out object dictionaryValue)
        {
            if (value is DictionaryEntry entry)
            {
                key = entry.Key;
                dictionaryValue = entry.Value;
                return true;
            }

            return TryReadKeyValuePair(value, out key, out dictionaryValue);
        }

        private static bool TryReadKeyValuePair(object value, out object key, out object dictionaryValue)
        {
            var type = value?.GetType();
            if (type?.IsGenericType != true || type.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            {
                key = null;
                dictionaryValue = null;
                return false;
            }

            key = type.GetProperty("Key")?.GetValue(value);
            dictionaryValue = type.GetProperty("Value")?.GetValue(value);
            return true;
        }

        public readonly record struct Options(int Depth = 0, Func<string, bool> Except = null, string KeyPrefix = "");
    }
}
