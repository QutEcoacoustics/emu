// <copyright file="Configuration.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.SongMeterMiniOrMicro
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.Enums;
    using Emu.Models;
    using NodaTime;

    /// <summary>
    /// A SMMS configuration.
    /// </summary>
    public record Configuration
    {
        public string IntendedModel { get; init; }

        /// <remarks>
        /// Equivalent to "prefix" in other models.
        /// </remarks>
        public string RecorderName { get; init; } = string.Empty;

        /// <remarks>
        /// This is not a timezone but rather a fixed offset.
        /// I'd normally name this "Offset" but
        /// I'm trying to make this structure match the UI of
        /// the configurator software.
        /// </remarks>
        public Offset Timezone { get; init; }

        public Location Position { get; init; }

        public uint SampleRateLeftChannel { get; init; }

        public uint SampleRateRightChannel { get; init; }

        public uint FullSpectrumSampleRate { get; init; }

        public Duration MaximumRecordingLength { get; init; }

        public Channel Channels { get; init; }

        /// <summary>
        /// Gets the unknown47 field.
        /// </summary>
        /// <remarks>
        /// Unknown, always observed to be 0x02 or 0x000 (for bat devices?)
        /// </remarks>
        public ushort Unknown47 { get; init; }

        public byte GainLeftDecibels { get; init; }

        public byte GainRightDecibels { get; init; }

        public RecordingFormat RecordingFormat { get; init; }

        public int MinimumTriggerFrequency { get; init; }

        public byte Unknown52 { get; init; } = 128;

        public byte Unknown53 { get; init; } = 15;

        public byte Unknown54 { get; init; } = 0;

        public byte Unknown55 { get; init; } = 0;

        public byte Unknown56 { get; init; } = 0;

        public bool SaveNoiseFiles { get; init; }

        public Duration TriggerWindow { get; init; }

        public Duration MaximumTriggerRecordingLength { get; init; }

        public byte Unknown61 { get; init; } = 1;

        public bool NonTriggeredRecording { get; init; }

        public RecordingQualityMode RecordingMode { get; init; }
    }
}
