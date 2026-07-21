// <copyright file="GuanoEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using LanguageExt;

    public record GuanoEntry
    {
        public Seq<string> Namespaces { get; init; } = Seq.empty<string>();

        public string Field { get; init; }

        public string Value { get; init; }

        public string Key => this.Namespaces.IsEmpty
            ? this.Field
            : string.Join("|", this.Namespaces) + "|" + this.Field;
    }
}
