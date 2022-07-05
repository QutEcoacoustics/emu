// <copyright file="OutputSink.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Cli
{
    using System.IO.Abstractions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class OutputSink
    {
        private readonly ILogger<OutputSink> logger;
        private readonly EmuGlobalOptions options;
        private readonly IFileSystem fileSystem;

        public OutputSink(ILogger<OutputSink> logger, EmuGlobalOptions options, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.options = options;
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Creates a text writer given an instance of an ouptput sink.
        /// This method is just glue for the dependency injection system.
        /// </summary>
        public static TextWriter Create(IServiceProvider provider)
        {
            return provider.GetRequiredService<OutputSink>().Create();
        }

        /// <summary>
        /// Create an output sink.
        /// </summary>
        /// <returns>An open stream that can be writtern to.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the output file exists and clobber is not specified in global options.
        /// </exception>
        public TextWriter Create()
        {
            if (this.options.Output is null)
            {
                // IConsole does not expose access to the text writer!
                return Console.Out;
            }
            else
            {
                var file = this.fileSystem.FileInfo.FromFileName(
                    this.fileSystem.Path.Combine(
                        this.fileSystem.Directory.GetCurrentDirectory(),
                        this.fileSystem.Path.GetFullPath(this.options.Output)));

                var directory = file.Directory;
                if (!directory.Exists)
                {
                    // create nested directories
                    directory.Create();
                }

                if (file.Exists)
                {
                    if (this.options.Clobber is true)
                    {
                        this.logger.LogWarning("Overwriting {ouput} because --clobber was specified", file);
                        file.Delete();
                    }
                    else
                    {
                        // if you're here you failed to validate input
                        throw new InvalidOperationException("File exists and clobber not specified");
                    }
                }

                return new StreamWriter(file.OpenWrite());
            }
        }
    }
}
