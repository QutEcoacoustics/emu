// <copyright file="GuanoKey.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.Standards.GUANO
{
    using LanguageExt;

    public record struct GuanoKey(Seq<string> Keys)
    {
        public GuanoKey(params string[] keys)
            : this(keys.ToSeq())
        {
        }

        public readonly Seq<string> Namespaces => this.Keys.Take(this.Keys.Count - 1);

        public readonly string Vendor => this.Namespaces.Any() ? this.Namespaces[0] : null;

        public readonly string Field => this.Keys.Last();

        public readonly bool IsVendorNamespace => this.Namespaces.Any() && this.Namespaces[0] != "GUANO";

        public readonly bool IsGuanoNamespace => this.Namespaces.Any() && this.Namespaces[0] == GuanoParser.GuanoNamespace;

        public readonly string FullKey => string.Join(GuanoParser.NamespaceSeparator.ToString(), this.Keys);

        public override readonly string ToString()
        {
            return this.FullKey;
        }

        public static GuanoKey Parse(string fullKey)
        {
            var keys = fullKey.Split(GuanoParser.NamespaceSeparator);
            return new GuanoKey(keys);
        }
    }
}
