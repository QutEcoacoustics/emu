// <copyright file="OperationInfo.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Fixes
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using LanguageExt;
    using Newtonsoft.Json;
    using static LanguageExt.Prelude;

    public partial record OperationInfo(
        WellKnownProblem Problem,
        bool Fixable,
        bool Safe,
        bool Automatic,
        [property: JsonIgnore]
        [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Type FixClass,
        Option<string> Suffix = default)
    {
        public string GetErrorName(IFileSystem fileSystem, string path)
        {
            var suffix = this.Suffix.IfNone(this.Problem.Id);
            var basename = fileSystem.Path.GetFileName(path);
            return $"{basename}.error_{suffix}";
        }
    }
}
