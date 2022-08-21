// <copyright file="MetadataBlockType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio
{
    public static partial class Flac
    {
        /// <summary>
        /// https://xiph.org/flac/format.html#metadata_block_header.
        /// </summary>
        public enum MetadataBlockType : byte
        {
            StreamInfo = 0,
            Padding = 1,
            Application = 2,
            SeekTable = 3,
            VorbisComment = 4,
            CueSheet = 5,
            Picture = 6,
        }
    }
}
