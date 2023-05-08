// <copyright file="ConfigFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.SupportFiles.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Emu.Audio.Vendors.OpenAcousticDevices;
    using LanguageExt;
    using LanguageExt.Common;
    using static LanguageExt.Prelude;

    public partial class ConfigFile : SupportFile
    {
        public const string Key = "CONFIG.TXT";
        public const string Pattern = "CONFIG.TXT";
        public const string FirmwareKey = "Firmware";
        public static readonly Error NotAConfigFile = Error.New("CONFIG.TXT does not have device ID");

        public ConfigFile(string path, Dictionary<string, object> data)
            : base(path)
        {
            this.Data = data;
        }

        public Dictionary<string, object> Data { get; }

        public static Option<SupportFile> Choose(TargetInformation target, IReadOnlyCollection<SupportFile> supportFiles)
        {
            if (supportFiles.Count == 1)
            {
                return supportFiles.Single();
            }

            throw new NotSupportedException("Support for multiple CONFIG.TXT files in one folder is not supported." + Meta.CallToAction);
        }

        public static Fin<SupportFile> Create(IFileSystem fileSystem, string path)
        {
            using var stream = fileSystem.File.OpenRead(path);
            return ConfigParser
                .IsConfigFile(stream, path)
                .Bind(success => success ? ConfigParser.Parse(stream) : NotAConfigFile)
                .Map<SupportFile>(x => new ConfigFile(path, x));
        }

        public Option<(string Version, string Name)> GetFirmware()
        {
            if (this.Data.TryGetValue(FirmwareKey, out var firmware))
            {
                var match = FirmwareParser().Match((string)firmware);
                if (match.Success)
                {
                    return (match.Groups[2].Value, match.Groups[1].Value);
                }
            }

            return None;
        }

        [GeneratedRegex(@"(.*) \(([.\d]+)\)")]
        private static partial Regex FirmwareParser();
    }
}
