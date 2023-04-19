// <copyright file="CompactFormatter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli.ObjectFormatters
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using YamlDotNet.Core.Tokens;

    public class CompactFormatter
        : ObjectFormatter
    {
        protected override void Append(StringBuilder builder, string key, string value, in Options options)
        {
            builder.AppendFormat("{0}={1};", key, value);
        }

        protected override string StyleKey(string key, object value)
        {
            return key;
        }

        protected override string StyleValue(object value, string key, string converted)
        {
            return converted.Replace(";", "\\;");
        }

        protected override Options StartList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options)
        {
            if (!complex)
            {
                var styledKey = this.StyleKey(key, list);
                builder.AppendFormat("{0}=[", styledKey);
            }
            else if (list.Count > 0)
            {
                return options with { KeyPrefix = key + "." };
            }

            return options;
        }

        protected override void EndList(StringBuilder builder, string key, IReadOnlyList<object> list, bool complex, in Options options)
        {
            if (!complex)
            {
                builder.Append("];");
            }
        }

        protected override Options StartObject(StringBuilder builder, string key, object obj, Type type, in Options options)
        {
            return options with { KeyPrefix = key + "." };
        }

        protected override void EndObject(StringBuilder builder, string key, object obj, Type type, in Options options)
        {
        }
    }
}
