// <copyright file="StringExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System;

using global::System.IO.Abstractions;
using global::System.Text;

public static class StringExtensions
{
    public static string FormatInlineList<T>(this IEnumerable<T> items, string delimiter = ",", string prefix = "", string suffix = "")
    {
        var builder = new StringBuilder();
        builder.Append(prefix);

        var enumerator = items.GetEnumerator();
        bool any = false;
        while (enumerator.MoveNext())
        {
            any = true;

            builder.Append(enumerator.Current);

            builder.Append(delimiter);
        }

        if (any)
        {
            builder.Remove(builder.Length - 1, delimiter.Length);
        }

        builder.Append(suffix);

        return builder.ToString();
    }

    public static DirectoryInfo ToDirectory(this string directory)
    {
        return new DirectoryInfo(directory);
    }

    public static IDirectoryInfo ToDirectory(this string directory, IFileSystem fileSystem)
    {
        return fileSystem.DirectoryInfo.FromDirectoryName(directory);
    }
}
