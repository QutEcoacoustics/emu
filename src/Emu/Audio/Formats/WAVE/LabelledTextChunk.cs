// <copyright file="LabelledTextChunk.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public record struct LabelledTextChunk : ICueWithText
    {
        public uint CuePointId;

        public uint SampleLength;

        public uint PurposeId;

        public uint Country;

        public uint Language;

        public uint Dialect;

        public uint CodePage;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Text;

        uint ICueWithText.CuePointId => this.CuePointId;

        string ICueWithText.Text => this.Text;
    }
}
