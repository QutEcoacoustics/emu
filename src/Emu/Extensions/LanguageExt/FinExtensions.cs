// <copyright file="FinExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace LanguageExt
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;
    using LanguageExt.Common;

    public static class FinExtensions
    {
        public static T IfFailDefault<T>(this in Fin<T> value)
        {
            return value.IsFail ? default : (T)value;
        }

        public static T? IfFailNullable<T>(this in Fin<T> value)
            where T : struct
        {
            return value.IsFail ? null : (T)value;
        }

        public static T IfFailNull<T>(this in Fin<T> value)
            where T : class
        {
            return value.IsFail ? null : (T)value;
        }

        // stand in until
        // https://github.com/louthy/language-ext/issues/1257
        // is resolved
        public static Seq<Error> Fails<T>(this in Fin<T> value)
        {
            if (value.IsFail)
            {
                return Seq.create<Error>((Error)value);
            }

            return Seq.empty<Error>();
        }
    }
}
