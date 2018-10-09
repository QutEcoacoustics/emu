// <copyright file="EmuEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
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
        /// <param name="args">The args array recieved by the executable.</param>
        public static void Main(string[] args)
        {
            BuildDependencies();

            Console.WriteLine($"Hello World!");
        }

        private static IServiceCollection BuildDependencies()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISerializer, CsvSerializer>();

            return serviceProvider;
        }
    }
}
