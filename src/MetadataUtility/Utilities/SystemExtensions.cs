// <copyright file="SystemExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
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
    }
}
