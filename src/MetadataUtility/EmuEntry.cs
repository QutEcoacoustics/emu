// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DotNet.Globbing;
    using McMaster.Extensions.CommandLineUtils;
    using MetadataUtility.Filenames;
    using MetadataUtility.Serialization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The main entry point for running EMU.
    /// </summary>
    public class EmuEntry
    {
        /// <summary>
        /// Run EMU with commandline arguments.
        /// </summary>
        /// <param name="args">The args array received by the executable.</param>
        public static async Task<int> Main(string[] args)
        {
            BuildDependencies();

            Console.WriteLine($"Hello World!");

            return await ProcessArguments(args);
        }

        private static IServiceCollection BuildDependencies()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISerializer, CsvSerializer>()
                .AddSingleton(typeof(FilenameParser), provider => FilenameParser.Default);

            return serviceProvider;
        }

        public static async Task<int> ProcessArguments(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            app.Argument<string>(
                "Recordings",
                "The recordings to process",
                true);

            app.OnExecute(
                async () => { return 1; });

            return app.Execute(args);
        }

        public static IEnumerable<string> ExpandGlobs(string baseDir, string[] patterns)
        {
            var globs = patterns.Select(Glob.Parse).ToArray();

            foreach (var dir in Directory.EnumerateFileSystemEntries(baseDir))
            {
                foreach (var glob in globs)
                {
                    if (glob.IsMatch(dir))
                    {
                        yield return dir;
                        break;
                    }
                }
            }
        }

        public static async void ProcessFile(string path)
        {

        }

    }
}
