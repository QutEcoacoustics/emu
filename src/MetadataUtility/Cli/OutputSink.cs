// <copyright file="OutputSink.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using Microsoft.Extensions.DependencyInjection;

    public static class OutputSink
    {
        public static Func<IServiceProvider, TextWriter> Factory => (collection) =>
        {
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
                        File.Delete(handler.Output);
                    }
                    else
                    {
                        throw new Exception("Will overwrite existing file, use --clobber option or select a different name");
                    }
                }

                return new StreamWriter(File.OpenWrite(handler.Output));
            }
        };
    }
}
