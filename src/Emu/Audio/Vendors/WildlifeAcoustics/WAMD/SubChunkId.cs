// <copyright file="SubChunkId.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.WAMD
{
    using System.Runtime.Serialization;

    // from wa_meta.h
    public enum SubChunkId : ushort
    {
        [EnumMember(Value = "METATAG_VERSION")]
        Version = 0x0000,
        [EnumMember(Value = "METATAG_DEV_MODEL")]
        DevModel = 0x0001,
        [EnumMember(Value = "METATAG_DEV_SERIAL_NUM")]
        DevSerialNum = 0x0002,
        [EnumMember(Value = "METATAG_SW_VERSION")]
        SwVersion = 0x0003,
        [EnumMember(Value = "METATAG_DEV_NAME")]
        DevName = 0x0004,
        [EnumMember(Value = "METATAG_FILE_START_TIME")]
        FileStartTime = 0x0005,
        [EnumMember(Value = "METATAG_GPS_FIRST")]
        GpsFirst = 0x0006,
        [EnumMember(Value = "METATAG_GPS_LAST")]
        GpsTrack = 0x0007,
        [EnumMember(Value = "METATAG_SOFTWARE")]
        Software = 0x0008,
        [EnumMember(Value = "METATAG_LICENSE_ID")]
        LicenseId = 0x0009,
        [EnumMember(Value = "METATAG_USER_NOTES")]
        UserNotes = 0x000A,
        [EnumMember(Value = "METATAG_AUTO_ID")]
        AutoId = 0x000B,
        [EnumMember(Value = "METATAG_ID_MANUAL")]
        ManualId = 0x000C,
        [EnumMember(Value = "METATAG_VOICE_NOTE")]
        VoiceNote = 0x000D,
        [EnumMember(Value = "METATAG_AUTO_ID_STATS")]
        AutoIdStats = 0x000E,
        [EnumMember(Value = "METATAG_TIME_EXPANSION")]
        TimeExpansion = 0x000F,
        [EnumMember(Value = "METATAG_DEV_PARAMS")]
        DevParams = 0x0010,
        [EnumMember(Value = "METATAG_DEV_RUNSTATE")]
        DevRunstate = 0x0011,
        [EnumMember(Value = "METATAG_MIC_TYPE")]
        MicType = 0x0012,
        [EnumMember(Value = "METATAG_MIC_SENSITIVITY")]
        MicSensitivity = 0x0013,
        [EnumMember(Value = "METATAG_POS_FIRST")]
        PosLast = 0x0014,
        [EnumMember(Value = "METATAG_TEMP_INT")]
        TempInt = 0x0015,
        [EnumMember(Value = "METATAG_TEMP_EXT")]
        TempExt = 0x0016,
        [EnumMember(Value = "METATAG_HUMIDITY")]
        Humidity = 0x0017,
        [EnumMember(Value = "METATAG_LIGHT")]
        Light = 0x0018,
        [EnumMember(Value = "METATAG_PRESSURE")]
        Padding = 0xFFFF,
    }
}
