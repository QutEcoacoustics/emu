// <copyright file="AdvancedScheduleEntryType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs
{
    public enum AdvancedScheduleEntryType : byte
    {
        HPF = 0x02,
        GAIN = 0x03,
        FS = 0x04,
        ZC = 0x05,
        FREQMIN = 0x06,
        FREQMAX = 0x07,
        DMIN = 0x08,
        DMAX = 0x09,
        TRGLVL = 0x0A,
        TRGWIN = 0x0B,
        TRGMAX = 0x0C,
        NAP = 0x0D,
        AT_DATE = 0x0E,
        AT_TIME = 0x0F,
        AT_SRIS = 0x10,
        AT_SSET = 0x11,
        REPEAT = 0x12,
        UNTDATE = 0x13,
        UNTTIME = 0x14,
        UNTSRIS = 0x15,
        UNTSSET = 0x16,
        UNTCOUNT = 0x17,
        RECORD = 0x18,
        PAUSE = 0x19,
        PLAY = 0x1A,
        FEATURE = 0x1B,
    }
}
