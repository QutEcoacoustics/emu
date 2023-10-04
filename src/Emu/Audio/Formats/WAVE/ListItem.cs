// <copyright file="ListItem.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    using System.Text;

    public partial record ListItem(byte[] Type, string Text);

    public partial record ListItem
    {
        public string TypeName => Encoding.ASCII.GetString(this.Type);
    }
}
