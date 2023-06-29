// <copyright file="ConfigParserTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Audio.Vendors.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu;
    using Emu.Audio.Vendors.OpenAcousticDevices;
    using Emu.Tests.TestHelpers;
    using FluentAssertions;
    using NodaTime;
    using Xunit.Abstractions;

    public class ConfigParserTests : TestBase
    {
        public static readonly TheoryData<string, Dictionary<string, object>> TestPaths = new()
        {
            {
                "OAD_AM/1.4.4_Normal/CONFIG.TXT",
                new()
                {
                    { "Device ID", "243B1F075C7A37BE" },
                    { "Firmware", "AudioMoth-Firmware-Basic (1.4.4)" },
                    { "Time zone",  Offset.FromHoursAndMinutes(10, 30) },
                    { "Sample rate (Hz)", 32000 },
                    { "Gain", GainSetting.MediumHigh },
                    { "Sleep duration (s)", null },
                    { "Recording duration (s)", null },
                    { "Active recording periods", 1 },
                    { "Recording period 1", "08:00 - 11:00 (UTC)" },
                    { "Earliest recording time", null },
                    { "Latest recording time", null },
                    { "Filter", null },
                    { "Amplitude threshold", null },
                    { "Enable LED", true },
                    { "Enable low-voltage cutoff", true },
                    { "Enable battery level indication", true },
                }
            },
            {
                "OAD_AM/1.8.0_Normal/CONFIG.TXT",
                new()
                {
                    { "Device ID", "24F3190361DA3578" },
                    { "Firmware", "AudioMoth-Firmware-Basic (1.8.0)" },
                    { "Time zone", Offset.FromHours(10) },
                    { "Sample rate (Hz)", 48000 },
                    { "Gain", GainSetting.Medium },
                    { "Sleep duration (s)", 270 },
                    { "Recording duration (s)", 30 },
                    { "Active recording periods", 1 },
                    { "Recording period 1", "00:00 - 24:00 (UTC)" },
                    { "Earliest recording time", null },
                    { "Latest recording time", null },
                    { "Filter", null },
                    { "Trigger type", null },
                    { "Threshold setting", null },
                    { "Minimum trigger duration (s)", null },
                    { "Enable LED", true },
                    { "Enable low-voltage cut-off", true },
                    { "Enable battery level indication", true },
                    { "Always require acoustic chime", false },
                    { "Use daily folder for WAV files", true },
                    { "Disable 48Hz DC blocking filter", false },
                    { "Enable energy saver mode", false },
                    { "Enable low gain range", false },
                    { "Enable magnetic switch", false },
                    { "Enable GPS time setting", false },
                }
            },
        };

        public ConfigParserTests(ITestOutputHelper output)
            : base(output, true)
        {
        }

        [Theory]
        [MemberData(nameof(TestPaths))]
        public void ConfigParserWorks(string path, Dictionary<string, object> expected)
        {
            var fullPath = this.CurrentFileSystem.Path.Combine(Helpers.FixturesRoot, path);
            using var stream = this.CurrentFileSystem.File.OpenRead(fullPath);

            Assert.True(ConfigParser.IsConfigFile(stream, path).IfFail(false));

            var result = ConfigParser.Parse(stream);

            Assert.True(result.IsSucc);

            result.ThrowIfFail().Should().BeEquivalentTo(expected);
        }
    }
}
