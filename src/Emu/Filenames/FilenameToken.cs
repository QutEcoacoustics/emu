// <copyright file="FilenameToken.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Filenames
{
    public abstract record FilenameToken
    {
        public record Literal(string Text) : FilenameToken
        {
            public override string ToString() => this.Text;
        }

        public record Value(string Name, string Prefix = "", bool Compact = false) : FilenameToken
        {
            public override string ToString() => $"{this.Prefix}{this.Name.AsToken()}";
        }
    }
}
