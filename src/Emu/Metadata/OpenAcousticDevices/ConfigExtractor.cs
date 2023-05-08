// <copyright file="ConfigExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.OpenAcousticDevices;
    using Emu.Metadata.SupportFiles.OpenAcousticDevices;
    using Emu.Models;
    using static LanguageExt.Prelude;

    public class ConfigExtractor : IMetadataOperation
    {
        public string Name { get; } = "CONFIG.TXT";

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            return ValueTask.FromResult(information.HasOadConfigFile());
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var configFile = (ConfigFile)information.TargetSupportFiles[ConfigFile.Key];

            var firmware = configFile.GetFirmware();

            // this is pretty minimal for now; we really only want the firmware value
            // which isn't included in the AudioMoth header comment.
            // There are other potential fields that might be worth extracting later on.
            var modified = recording with
            {
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Firmware = firmware.Map(f => f.Version).IfNoneUnsafe(recording.Sensor?.Firmware),
                    FirmwareName = firmware.Map(f => f.Name).IfNoneUnsafe(recording.Sensor?.FirmwareName),
                },
            };

            return ValueTask.FromResult(modified);
        }
    }
}
