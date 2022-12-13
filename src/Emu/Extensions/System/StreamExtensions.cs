// <copyright file="StreamExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System.IO
{
    using LanguageExt;
    using LanguageExt.Common;

    public static class StreamExtensions
    {
        public static readonly Error SeekFailed = Error.New("Failed to seek stream");

        public static Fin<long> SeekSafe(this Stream stream, long position, Error error = null)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            var offset = stream.Seek(position, SeekOrigin.Begin);
            if (offset != position)
            {
                return error ?? SeekFailed;
            }

            return offset;
        }
    }
}
