// <copyright file="Guano.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.TitleyScientific
{
    using System.Globalization;
    using Emu.Audio.Standards.GUANO;
    using Emu.Models;

    public static class Guano
    {
        public const string BatteryVoltageField = "Battery voltage";
        public const string GainField = "Gain (A)";
        public const string MicrophoneField = "Microphone";
        public const string Namespace = "Anabat";

        public static Recording ApplyVendorEntries(GuanoBlock guano, Recording recording)
        {
            var voltage = ParseDouble(guano.GetValue(new GuanoKey(Namespace, BatteryVoltageField)));
            var microphoneType = guano.GetValue(new GuanoKey(Namespace, MicrophoneField));
            var microphoneGain = ParseDouble(guano.GetValue(new GuanoKey(Namespace, GainField)));
            var sensor = recording.Sensor ?? new Sensor();

            if (microphoneType is not null || microphoneGain is not null)
            {
                var microphone = sensor.Microphones?.FirstOrDefault() ?? new Microphone();
                sensor = sensor with
                {
                    Microphones =
                    [
                        microphone with
                        {
                            Type = microphoneType ?? microphone.Type,
                            Gain = microphoneGain ?? microphone.Gain,
                            Channel = microphone.Channel ?? 0,
                        },
                    ],
                };
            }

            return recording with
            {
                Sensor = sensor with { Voltage = voltage ?? sensor.Voltage },
            };
        }

        private static double? ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }
    }
}
