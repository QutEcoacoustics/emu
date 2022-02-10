// <copyright file="OutputSink.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    public class OutputSink
    {
        public static Func<IServiceProvider, TextWriter> Factory => (collection) =>
        {
            // ILogger<OutputSink> logger = new ILogger<OutputSink>();

            var logger = collection.GetRequiredService<ILogger<OutputSink>>();

            var handler = collection.GetRequiredService<EmuGlobalOptions>();

            if (handler.Output is null)
            {
                // IConsole does not expose access to the text writer!
                return Console.Out;
            }
            else
            {
                if (File.Exists(handler.Output))
                {
                    if (handler.Clobber is true)
                    {
                        logger.LogInformation($"Deleting {handler.Output} for overwrite");
                        File.Delete(handler.Output);
                    }
                    else
                    {
                        throw new Exception($"Will not overwrite existing output file {handler.Output}, use --clobber option or select a different name");
                    }
                }

                return new StreamWriter(File.OpenWrite(handler.Output));
            }
        };
    }
}
