// <copyright file="Guano.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.WildlifeAcoustics
{
    using Emu.Audio.Standards.GUANO;
    using Emu.Models;
    using Newtonsoft.Json.Linq;

    public static class Guano
    {
        public const string AlternateMake = "Wildlife Acoustics, Inc";
        public const string AudioSettingsField = "Audio settings";
        public const string Make = "Wildlife Acoustics";
        public const string Namespace = "WA";
        public const string SongMeterNamespace = "Song Meter";

        public static Recording ApplyVendorEntries(GuanoBlock guano, Recording recording)
        {
            var value = guano.GetValue([Namespace, SongMeterNamespace], AudioSettingsField);
            if (string.IsNullOrWhiteSpace(value))
            {
                return recording;
            }

            var settings = JArray.Parse(value);
            var existingMicrophones = recording.Sensor?.Microphones ?? Array.Empty<Microphone>();
            var microphones = settings
                .Select((setting, index) =>
                {
                    var existing = existingMicrophones.ElementAtOrDefault(index) ?? new Microphone();
                    var type = setting.Value<string>("mic");
                    return existing with
                    {
                        Type = type ?? existing.Type,
                        Gain = setting.Value<double?>("gain") ?? existing.Gain,
                        Channel = existing.Channel ?? index,
                    };
                })
                .ToArray();

            return recording with
            {
                Sensor = (recording.Sensor ?? new Sensor()) with { Microphones = microphones },
            };
        }


    }
}
