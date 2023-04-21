// <copyright file="SystemExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    /// Extension methods for the <see cref="System"/> namespace.
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Format a string template with values.
        /// </summary>
        /// <remarks>
        /// This is a thin wrapper around <see cref="string.Format(string, object)"/>.
        /// </remarks>
        /// <param name="target">The string value to template.</param>
        /// <param name="arguments">The values to format into the template.</param>
        /// <returns>The templated string.</returns>
        public static string Template(this string target, params object[] arguments)
        {
            return string.Format(target, arguments);
        }

        /// <summary>
        /// Wraps the <paramref name="item"/> in an array.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="item"/>.</typeparam>
        /// <param name="item">The item to wrap.</param>
        /// <returns>An array with one item.</returns>
        public static T[] AsArray<T>(this T item)
        {
            return new[] { item };
        }

        /// <summary>
        /// Wraps the <paramref name="item"/> in an enumerable sequence.
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="item"/>.</typeparam>
        /// <param name="item">The item to wrap.</param>
        /// <returns>An enumerable with one item.</returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        // https://stackoverflow.com/a/64307613/224512
        // record structs are not detectable
        public static bool IsRecordClass(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] this Type type)
            => type.GetMethod("<Clone>$") != null;

        public static string GetEnumMemberValueOrDefault
            <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this T value)
            where T : Enum
        {
            var basic = value.ToString();
            var members = typeof(T).GetTypeInfo().DeclaredMembers;
            return members
                .SingleOrDefault(x => x.Name == basic)
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value ?? basic;
        }
    }
}
