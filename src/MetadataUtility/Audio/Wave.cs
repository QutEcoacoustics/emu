// <copyright file="Wave.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio.Vendors
{
    public static class Wave
    {
        public static readonly byte[] WaveMagicNumber = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };

        public static readonly byte[] DataBlockId = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
    }
}
