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

    public static string[] SplitLines(this string input)
    {
        return input.Split(new char[] { '\r', '\n' }, StringSplitOptions.None);
    }

    public static DirectoryInfo ToDirectory(this string directory)
    {
#pragma warning disable IO0007 // Replace DirectoryInfo class with IFileSystem.DirectoryInfo for improved testability
        return new DirectoryInfo(directory);
#pragma warning restore IO0007 // Replace DirectoryInfo class with IFileSystem.DirectoryInfo for improved testability
    }

    public static IDirectoryInfo ToDirectory(this string directory, IFileSystem fileSystem)
    {
        return fileSystem.DirectoryInfo.FromDirectoryName(directory);
    }

    /// <summary>
    /// Creates an empty file, and any intermediate directories.
    /// </summary>
    /// <param name="path">The file to create.</param>
    /// <param name="fileSystem">The file system to operate on.</param>
    public static string Touch(this string path, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(path, nameof(path));

        var directory = fileSystem.Path.GetDirectoryName(path);
        fileSystem.Directory.CreateDirectory(directory);
        fileSystem.File.Create(path).Close();

        return path;
    }

    public static string AsToken(this string input)
    {
        return $"{{{input}}}";
    }
}
