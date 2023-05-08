// <copyright file="GitInfo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

// fullnamespace shenanigans because of https://github.com/devlooped/GitInfo/issues/273
[assembly: System.Reflection.AssemblyVersion(ThisAssembly.AssemblyVersion)]

[assembly: System.Reflection.AssemblyFileVersion(ThisAssembly.AssemblyVersion)]

[assembly: System.Reflection.AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]

internal partial class ThisAssembly
{
    public const string InformationalVersion = $"{System.Reflection.ThisAssembly.Git.SemVer.Major}.{System.Reflection.ThisAssembly.Git.SemVer.Minor}.{System.Reflection.ThisAssembly.Git.SemVer.Patch}-{System.Reflection.ThisAssembly.Git.Commits}+g{System.Reflection.ThisAssembly.Git.Commit}";

    public const string AssemblyVersion = $"{System.Reflection.ThisAssembly.Git.SemVer.Major}.{System.Reflection.ThisAssembly.Git.SemVer.Minor}.{System.Reflection.ThisAssembly.Git.SemVer.Patch}";
}
