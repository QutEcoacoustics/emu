// <copyright file="AudioMothCommentExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata.OpenAcousticDevices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Emu.Audio;
    using Emu.Audio.Vendors.OpenAcousticDevices;
    using Emu.Filenames;
    using Emu.Models;
    using LanguageExt;
    using LanguageExt.Common;
    using Microsoft.Extensions.Logging;

    public class AudioMothCommentExtractor : IRawMetadataOperation
    {
        private readonly ILogger<AudioMothCommentExtractor> logger;
        private readonly FilenameParser parser;

        public AudioMothCommentExtractor(ILogger<AudioMothCommentExtractor> logger, FilenameParser parser)
        {
            this.logger = logger;
            this.parser = parser;
        }

        public string Name { get; } = "AudioMothArtistAndComment";

        public ValueTask<bool> CanProcessAsync(TargetInformation information)
        {
            var result = information.IsPcmWaveFile() && information.HasAudioMothListChunk();

            return ValueTask.FromResult(result);
        }

        public ValueTask<object> ProcessFileAsync(TargetInformation information)
        {
            var result = this.parser.Parse(information.Path);

            var comment = AudioMothMetadataParser.ParseAudioMothListChunk(
                information.FileStream,
                result.LocalStartDate.ToOption());

            if (this.LogFailure(information, comment))
            {
                return ValueTask.FromResult<object>(null);
            }

            return ValueTask.FromResult<object>(comment.ThrowIfFail());
        }

        public ValueTask<Recording> ProcessFileAsync(TargetInformation information, Recording recording)
        {
            var commentMaybe = AudioMothMetadataParser.ParseAudioMothListChunk(
                information.FileStream,
                recording.LocalStartDate.ToOption());

            if (this.LogFailure(information, commentMaybe))
            {
                return ValueTask.FromResult<Recording>(recording);
            }

            var comment = (AudioMothComment)commentMaybe;

            var modified = recording with
            {
                StartDate = comment.Datestamp,
                TrueStartDate = comment.Datestamp,
                LocalStartDate = recording.LocalStartDate ?? comment.Datestamp.LocalDateTime,
                Sensor = (recording.Sensor ?? new Sensor()) with
                {
                    Make = Vendor.OpenAcousticDevices.GetEnumMemberValueOrDefault(),
                    Model = Models.AudioMoth,
                    Firmware = this.JoinFirmwares(comment.PossibleFirmwares),
                    SerialNumber = comment.SerialNumber,
                    Microphones = new Microphone()
                    {
                        Channel = 0,
                        ChannelName = "A",
                        Type = comment.ExternalMicrophone ? "External" : "Internal",
                        Gain = comment.Gain,
                    }.AsArray(),
                    Voltage = comment.Voltage,
                    Temperature = comment.Temperature,
                    Name = comment.DeploymentId ?? comment.SerialNumber,
                },
                RecordingStatus = comment.RecordingState.ToString(),
                Notices = recording.Notices.Concat(comment.Notices),
            };

            return ValueTask.FromResult(modified);
        }

        /// <summary>
        /// We guess at the firmwares so there's a range of possibilities.
        /// </summary>
        private string JoinFirmwares(Seq<Version> versions)
        {
            return versions.Count switch
            {
                0 => null,
                1 => versions.First().ToString(),
                _ => $"{versions.Min()}..{versions.Max()}",
            };
        }

        private bool LogFailure(TargetInformation information, Fin<AudioMothComment> comment)
        {
            if (comment.Case is Error e)
            {
                this.logger.Log(
                    e == AudioMothMetadataParser.CommentError ? LogLevel.Warning : LogLevel.Debug,
                    "Failed to read AudioMoth comment for {file}: {reason}",
                    information.Path,
                    e);

                return true;
            }

            return false;
        }
    }
}
