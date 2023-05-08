// <copyright file="PredicateExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata;

using Emu.Audio;
using Emu.Audio.Vendors.OpenAcousticDevices;
using Emu.Audio.Vendors.WildlifeAcoustics.WAMD;
using Emu.Audio.WAVE;
using Emu.Metadata.SupportFiles.FrontierLabs;
using Emu.Metadata.SupportFiles.OpenAcousticDevices;

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

    public static bool HasFrontierLabsVorbisComment(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasFrontierLabsVorbisComment);
    }

    public static bool HasBarltLogFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasBarltLogFile);
    }

    public static bool HasOadConfigFile(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasOadConfigFile);
    }

    public static bool IsPreallocatedHeader(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.IsPreallocatedHeader);
    }

    public static bool HasVersion1WamdChunk(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasVersion1WamdChunk);
    }

    public static bool HasAudioMothListChunk(this TargetInformation target)
    {
        return target.CheckPredicate(Predicates.HasAudioMothListChunk);
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

        public static readonly Func<TargetInformation, bool> HasBarltLogFile =
            target => target.TargetSupportFiles.ContainsKey(LogFile.LogFileKey);

        public static readonly Func<TargetInformation, bool> HasOadConfigFile =
            target => target.TargetSupportFiles.ContainsKey(ConfigFile.Key);

        public static readonly Func<TargetInformation, bool> HasFrontierLabsVorbisComment =
            target => Audio.Vendors.FrontierLabs.HasFrontierLabsVorbisComment(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> IsPreallocatedHeader =
            target => Audio.Vendors.FrontierLabs.IsPreallocatedHeader(target.FileStream, target.Path);

        public static readonly Func<TargetInformation, bool> HasVersion1WamdChunk =
            target => WamdParser.HasVersion1WamdChunk(target.FileStream).IfFail(false);

        public static readonly Func<TargetInformation, bool> HasAudioMothListChunk =
            target => AudioMothMetadataParser.HasAudioMothListChunk(target.FileStream).IfFail(false);

        // An example of an async predicate
        // public static readonly Func<TargetInformation, ValueTask<bool>> IsFlacFile2 =
        //      async target => Flac.IsFlacFile(target.FileStream).IfFail(false);
    }
}
