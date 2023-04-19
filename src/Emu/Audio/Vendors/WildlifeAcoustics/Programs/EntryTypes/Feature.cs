// <copyright file="Feature.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Vendors.WildlifeAcoustics.Programs.EntryTypes
{
    using Emu.Audio.Vendors.WildlifeAcoustics.Programs;
    using static Emu.Utilities.BinaryHelpers;

    public record Feature : AdvancedScheduleEntry
    {
        public Feature()
        {
            this.Type = AdvancedScheduleEntryType.FEATURE;
        }

        public string Id
        {
            get
            {
                return ReadBitRange(this.Raw, 0, 4) switch
                {
                    0 => "01 - LED DISABLE",
                    1 => "02 - 32BIT ENABLE",
                    uint i => (i + 1).ToString("dd"),
                };
            }

            init
            {
                byte feature = value switch
                {
                    "01 - LED DISABLE" => 0,
                    "02 - 32BIT ENABLE" => 1,
                    string s => byte.Parse(s),
                    _ => throw new NotImplementedException(),
                };

                WriteBitRange(ref this.raw, 0, 4, feature);
            }
        }

        public bool Enabled
        {
            get
            {
                return ReadBitRange(this.Raw, 4, 8) == 1;
            }

            init
            {
                WriteBitRange(ref this.raw, 4, 8, value ? 1u : 0);
            }
        }
    }
}
