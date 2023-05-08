// <copyright file="AudioMothComment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using Emu.Models.Notices;
    using LanguageExt;
    using NodaTime;

    public partial record AudioMothComment(
        Seq<Version> PossibleFirmwares = default,
        OffsetDateTime Datestamp = default,
        string SerialNumber = default,
        GainSetting GainSetting = default,
        BatteryLimit BatteryLevel = default,
        double Voltage = default,
        RecordingState RecordingState = default,
        double? Temperature = default,
        TriggerType TriggerType = default,
        double? AmplitudeTriggerThreshold = default,
        double? FrequencyTriggerCenter = default,
        double? FrequencyTriggerWindow = default,
        double? FrequencyTriggerThreshold = default,
        double? MinimumTriggerDuration = default,
        double? LowPassFilter = default,
        double? HighPassFilter = default,
        (double Low, double High)? BandPassFilter = default,
        string DeploymentId = default,
        bool ExternalMicrophone = false,
        Seq<Notice> Notices = default);

    public partial record AudioMothComment
    {
        public double Gain => this.GainSetting switch
        {
            // https://www.openacousticdevices.info/support/main/comment/59c63821-f6d3-37d5-9e16-fccb7829be7d?postId=606f0af298fb4e001508aab8
            // https://www.openacousticdevices.info/support/configuration-support/how-many-db-audiomoth-records-high-gain-mode
            // https://www.openacousticdevices.info/support/device-support/_gain
            GainSetting.Low => 4.33,
            GainSetting.LowMedium => 7,
            GainSetting.Medium => 15,
            GainSetting.MediumHigh => 25.1,
            GainSetting.High => 30,
            _ => throw new NotImplementedException(),
        };
    }
}
