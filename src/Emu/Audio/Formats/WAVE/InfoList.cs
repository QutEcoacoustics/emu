// <copyright file="InfoList.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    using System.Text;

    public record InfoList(IReadOnlyCollection<ListItem> Entries) : List();
}
