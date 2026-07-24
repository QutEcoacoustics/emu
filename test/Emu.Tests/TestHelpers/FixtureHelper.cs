// <copyright file="FixtureHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    public static partial class FixtureHelper
    {
        private static readonly string[] SupportedArchiveExtensions = new[] { ".zip" };

        public static IFileSystem RealFileSystem { get; } = new FileSystem();

        public static string ResolvePath(string name)
        {
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, name));
            return ResolvePathWithArchiveFallback(path, name);
        }

        public static string ResolveDirectory(string name)
        {
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, name));

            if (!RealFileSystem.Directory.Exists(path))
            {
                throw new FileNotFoundException($"Could not find name {name} at path {path}");
            }

            return path;
        }

        public static string ResolveFirstDirectory(string name)
        {
            // by convention all of the paths in our fixtures file uses `/`
            var firstDirectory = name.Split('/').First();
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, firstDirectory));

            if (!RealFileSystem.Directory.Exists(path))
            {
                throw new FileNotFoundException($"Could not find directory {firstDirectory} at path {path}");
            }

            return path;
        }

        internal static string ResolvePathWithArchiveFallback(string path, string name = null)
        {
            if (RealFileSystem.File.Exists(path))
            {
                return path;
            }

            foreach (var extension in SupportedArchiveExtensions)
            {
                var archivePath = path + extension;

                if (!RealFileSystem.File.Exists(archivePath))
                {
                    continue;
                }

                var fixtureName = name ?? path;
                throw new FileNotFoundException(
                    $"Could not find fixture '{fixtureName}' at path '{path}', but found sibling archive '{archivePath}'. " +
                    "Run `dotnet test` (without `--no-build`) or build `test/Emu.Tests/Emu.Tests.csproj` to prepare compressed fixtures first.");
            }

            var displayName = name ?? path;
            throw new FileNotFoundException($"Could not find name {displayName} at path {path}");
        }
    }
}
