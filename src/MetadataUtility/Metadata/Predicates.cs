// <copyright file="Predicates.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata;

using MetadataUtility.Audio;
using MetadataUtility.Metadata;

public static class Predicates
{
    public static readonly Func<TargetInformation, bool> HasFileName =
        target => !string.IsNullOrWhiteSpace(target.FileSystem.Path.GetFileName(target.Path));

    public static readonly Func<TargetInformation, bool> IsFlacFile =
        target => Flac.IsValidFlacFile(target.FileStream).IfFail(false);

    // An example of an async predicate
    // public static readonly Func<TargetInformation, ValueTask<bool>> IsFlacFile2 =
    //      async target => Flac.IsFlacFile(target.FileStream).IfFail(false);
}

public static class TargetInformationExtensions
{
    public static bool HasFileName(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasFileName);
    }

    public static bool IsFlacFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.IsFlacFile);
    }

    // an example of an async predicate extension method.
    // public static ValueTask<bool> IsFlacFileAsync(this TargetInformation target)
    // {
    //     return target.CheckPredicateAsync(Predicates.IsFlacFile2);
    // }
}
