// <copyright file="ExtensionInferer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.IO.Abstractions;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.WAVE;
    using Emu.Filenames;
    using Emu.Models;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Backfills the extension field if we know the mime type.
    /// Separate extractor so it can be removed from pipelines when needed.
    /// </summary>
    public class ExtensionInferer : IMetadataOperation
    {
        public ExtensionInferer()
        {
        }

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            return ValueTask.FromResult(true);
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            if (recording.Extension is null)
            {
                var extension = recording.MediaType switch
                {
                    null => null,
                    Wave.Mime => Wave.Extension,
                    Flac.Mime => Flac.Extension,
                    _ => throw new NotImplementedException(
                        $"Do not currently support inferring extension for media type `{recording.MediaType}`"),
                };

                recording = recording with
                {
                    Extension = extension,
                };
            }

            return ValueTask.FromResult(recording);
        }
    }
}
