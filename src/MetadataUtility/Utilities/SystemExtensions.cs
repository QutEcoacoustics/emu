// <copyright file="SystemExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

// ReSharper disable once CheckNamespace
namespace System
{
    public static class SystemExtensions
    {
        public static string Template(this string target, params object[] arguments)
        {
            return string.Format(target, arguments);
        }
    }
}
