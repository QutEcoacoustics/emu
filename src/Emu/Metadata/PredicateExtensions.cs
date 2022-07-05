// <copyright file="PredicateExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata;

using Emu.Audio;
using Emu.Metadata.SupportFiles.FrontierLabs;

public static class PredicateExtensions
{
    public static bool HasFileName(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasFileName);
    }

    public static bool IsFlacFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.IsFlacFile);
    }

    public static bool IsPcmWaveFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.IsPcmWaveFile);
    }

    public static bool HasMetadataBlock(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasMetadataBlock);
    }

    public static bool HasFrontierLabsVorbisComment(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasFrontierLabsVorbisComment);
    }

    public static bool HasBarltLogFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasBarltLogFile);
    }

    public static bool IsPreallocatedHeader(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.IsPreallocatedHeader);
    }

    public static bool HasVersion1WamdChunk(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasVersion1WamdChunk);
    }

    // an example of an async predicate extension method.
    // public static ValueTask<bool> IsFlacFileAsync(this TargetInformation target)
    // {
    //     return target.CheckPredicateAsync(Predicates.IsFlacFile2);
    // }

    public static class Predicates
    {
        public static readonly Func<TargetInformation, bool> HasFileName =
            target => !string.IsNullOrWhiteSpace(target.FileSystem.Path.GetFileName(target.Path));

        public static readonly Func<TargetInformation, bool> IsFlacFile =
            target => Flac.IsFlacFile(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> IsPcmWaveFile =
            target => Wave.IsPcmWaveFile(target.FileStream).IsSucc;

        public static readonly Func<TargetInformation, bool> HasMetadataBlock =
            target => Flac.HasMetadataBlock(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> HasBarltLogFile =
            target => target.TargetSupportFiles.ContainsKey(LogFile.LogFileKey);

        public static readonly Func<TargetInformation, bool> HasFrontierLabsVorbisComment =
            target => Audio.Vendors.FrontierLabs.HasFrontierLabsVorbisComment(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> IsPreallocatedHeader =
            target => Wave.IsPreallocatedHeader(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> HasVersion1WamdChunk =
            target => Wamd.HasVersion1WamdChunk(target.FileStream).IfFail(false);

        // An example of an async predicate
        // public static readonly Func<TargetInformation, ValueTask<bool>> IsFlacFile2 =
        //      async target => Flac.IsFlacFile(target.FileStream).IfFail(false);
    }
}
