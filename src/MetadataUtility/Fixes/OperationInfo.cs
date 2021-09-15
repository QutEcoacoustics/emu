// <copyright file="Fix.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

using Newtonsoft.Json;

namespace MetadataUtility.Fixes
{
    public partial record OperationInfo(
        WellKnownProblem Problem,
        bool Fixable,
        bool Safe,
        bool Automatic,
        [property: JsonIgnore] Type FixClass);
}
