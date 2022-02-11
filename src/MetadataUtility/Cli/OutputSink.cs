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
            var logger = collection.GetRequiredService<ILogger<OutputSink>>();

            var handler = collection.GetRequiredService<EmuGlobalOptions>();

            if (handler.Output is null)
            {
                // IConsole does not expose access to the text writer!
                return Console.Out;
            }
            else
            {
                if (File.Exists(handler.Output) && handler.Clobber is true)
                {
                    logger.LogInformation($"Deleting {handler.Output} for overwrite");
                    File.Delete(handler.Output);
                }

                return new StreamWriter(File.OpenWrite(handler.Output));
            }
        };
    }
}
