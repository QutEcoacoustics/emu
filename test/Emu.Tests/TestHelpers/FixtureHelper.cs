// <copyright file="FixtureHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.TestHelpers
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Xunit;

    public static partial class FixtureHelper
    {
        public static IFileSystem RealFileSystem { get; } = new FileSystem();

        public static string ResolvePath(string name)
        {
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, name));

            if (!RealFileSystem.File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find name {name} at path {path}");
            }

            return path;
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
            // be convention all of the paths in our fixtures CSV uses `/`
            var firstDirectory = name.Split('/').First();
            var path = RealFileSystem.Path.GetFullPath(RealFileSystem.Path.Combine(Helpers.FixturesRoot, firstDirectory));

            if (!RealFileSystem.Directory.Exists(path))
            {
                throw new FileNotFoundException($"Could not find directory {firstDirectory} at path {path}");
            }

            return path;
        }
    }
}
