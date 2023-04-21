// <copyright file="Play.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using System.Text.RegularExpressions;
    using static Emu.Utilities.BinaryHelpers;

    public record Play : AdvancedScheduleEntry
    {
        private static readonly Regex Name = new(@"^CALL(\d{1,2})\.WAV$", RegexOptions.Compiled);

        public Play()
        {
            this.Type = AdvancedScheduleEntryType.PLAY;
        }

        public string File
        {
            get
            {
                return $"CALL{ReadBitRange(this.Raw, 19, 23)}.WAV";
            }

            init
            {
                var match = Name.Match(value);

                if (!match.Success)
                {
                    throw new ArgumentException("Invalid file name", nameof(value));
                }

                WriteBitRange(ref this.raw, 19, 23, uint.Parse(match.Groups[1].Value));
            }
        }
    }
}
