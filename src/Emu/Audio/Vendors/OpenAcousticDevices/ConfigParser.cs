// <copyright file="ConfigParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes;
    using LanguageExt;
    using NodaTime;
    using NodaTime.Text;
    using static Emu.Audio.Vendors.OpenAcousticDevices.AudioMothMetadataParser;

    public static class ConfigParser
    {
        // present in every version of the config file, and should be near the top
        public static readonly byte[] SearchToken = "Device ID"u8.ToArray();

        private const string ScheduleLimitDefault = "---------- --:--:--";
        private const string Value = "CONFIG.TXT";

        public static Fin<bool> IsConfigFile(Stream stream, string path)
        {
            // https://github.com/OpenAcousticDevices/AudioMoth-Firmware-Basic/blame/master/src/main.c#L597
            if (!path.EndsWith(Value))
            {
                return false;
            }

            stream.Position = 0;
            Span<byte> buffer = stackalloc byte[1024];
            stream.Read(buffer);

            return buffer.IndexOf(SearchToken) >= 0;
        }

        public static Fin<Dictionary<string, object>> Parse(Stream stream)
        {
            stream.Position = 0;

            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);

            var result = new Dictionary<string, object>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var split = line.Split(':', 2, StringSplitOptions.TrimEntries);

                var key = split[0];
                var value = split[1];

                var parsedValue = ParseValue(value, key);

                result.Add(key, parsedValue);
            }

            return result;
        }

        private static object ParseValue(string value, string key)
        {
            if (string.IsNullOrWhiteSpace(value) || value is "-" or ScheduleLimitDefault)
            {
                return null;
            }

            // 1.4.x uses true/false and 1.8.x uses Yes/No - it changed somewhere in between
            if (value.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (value.Equals("No", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (key == "Gain")
            {
                var success = value.TryParseEnumMember<GainSetting>(ignoreCase: true, out var gain);

                if (success)
                {
                    return gain;
                }

                throw new FormatException($"Unknown gain setting found: `{value}`");
            }

            if (key == "Time zone")
            {
                value = value.StartsWith("UTC") ? value[3..] : value;
                var offset = OffsetPattern.GeneralInvariant.Parse(value);
                if (offset.Success)
                {
                    return offset.Value;
                }
            }

            if (int.TryParse(value, out var integer))
            {
                return integer;
            }

            if (double.TryParse(value, out var number))
            {
                return number;
            }

            return value;
        }
    }
}
