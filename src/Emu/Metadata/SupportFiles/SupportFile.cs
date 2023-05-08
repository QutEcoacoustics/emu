// <copyright file="SupportFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.SupportFiles
{
    using System;
    using System.IO.Abstractions;

    public abstract class SupportFile
    {
        public SupportFile(string path)
        {
            this.Path = path;
        }

        public string Path { get; }
    }
}
