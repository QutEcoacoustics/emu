// <copyright file="GitInfo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

using System.Reflection;

[assembly: AssemblyVersion(ThisAssembly.AssemblyVersion)]

[assembly: AssemblyFileVersion(ThisAssembly.AssemblyVersion)]

[assembly: AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]

internal partial class ThisAssembly
{
    public const string InformationalVersion = $"{Git.SemVer.Major}.{Git.SemVer.Minor}.{Git.SemVer.Patch}-{Git.Commits}+g{Git.Commit}";

    public const string AssemblyVersion = $"{Git.SemVer.Major}.{Git.SemVer.Minor}.{Git.SemVer.Patch}";
}
