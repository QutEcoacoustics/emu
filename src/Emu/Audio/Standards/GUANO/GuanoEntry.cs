// <copyright file="GuanoEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using System.Collections.Generic;

    public record GuanoEntry
    {
        public IReadOnlyList<string> Namespaces { get; init; } = [];

        public string Field { get; init; }

        public string Value { get; init; }
    }
}
