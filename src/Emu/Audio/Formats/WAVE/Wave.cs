// <copyright file="Wave.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Audio.WAVE
{
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using LanguageExt;
    using LanguageExt.Common;
    using static LanguageExt.Prelude;

    // http://soundfile.sapp.org/doc/WaveFormat/
    // http://www-mmsp.ece.mcgill.ca/Documents/AudioFormats/WAVE/WAVE.html
    // https://docs.microsoft.com/en-us/windows-hardware/drivers/audio/extensible-wave-format-descriptors
    // https://docs.microsoft.com/en-us/windows/desktop/api/mmreg/ns-mmreg-twaveformatex
    // https://tools.ietf.org/html/rfc2361
    // https://tools.ietf.org/html/draft-ema-vpim-wav-00
    // https://sites.google.com/site/musicgapi/technical-documents/wav-file-format#fact
    // https://web.archive.org/web/20081201144551/http://music.calarts.edu/~tre/PeakChunk.html
    // https://icculus.org/SDL_sound/downloads/external_documentation/wavecomp.htm
    // https://www.aelius.com/njh/wavemetatools/doc/riffmci.pdf
    // https://www.recordingblogs.com/wiki/associated-data-list-chunk-of-a-wave-file
    // http://www.piclist.com/techref/io/serial/midi/wave.html
    public static class Wave
    {
        public const string Mime = "audio/wave";
        public const string Extension = ".wav";

        public const int RiffLengthOffset = 4;
        public const int MinimumRiffHeaderLength = 8;
        public const int FL005ErrorBytes = 44;
        public const int ChunkIdLength = 4;

        public static readonly byte[] RiffMagicNumber = "RIFF"u8.ToArray();
        public static readonly byte[] WaveMagicNumber = "WAVE"u8.ToArray();
        public static readonly byte[] FormatChunkId = "fmt "u8.ToArray();
        public static readonly byte[] DataChunkId = "data"u8.ToArray();
        public static readonly byte[] CueChunkId = "cue "u8.ToArray();
        public static readonly byte[] InfoChunkId = "INFO"u8.ToArray();
        public static readonly byte[] InfoArtistChunkId = "IART"u8.ToArray();
        public static readonly byte[] InfoCommentChunkId = "ICMT"u8.ToArray();
        public static readonly byte[] ListChunkId = "LIST"u8.ToArray();
        public static readonly byte[] LabelChunkId = "labl"u8.ToArray();
        public static readonly byte[] NoteChunkId = "note"u8.ToArray();
        public static readonly byte[] LabelledTextChunkId = "ltxt"u8.ToArray();
        public static readonly byte[] AssociatedDataListChunkId = "adtl"u8.ToArray();

        public static readonly Error FileTooShortRiff = Error.New("Error reading file: file is not long enough to have RIFF/WAVE header");
        public static readonly Error FileNotWave = Error.New("Error reading file: file is not a RIFF/WAVE file");
        public static readonly Error InvalidFileData = Error.New("Error reading file: no valid file data was found");
        public static readonly Error InvalidOffset = Error.New("Error reading file: an invalid offset was found");
        public static readonly Error InvalidChunk = Error.New("Error reading chunk: chunk size exceeds file size");
        public static readonly Error NoCueChunk = Error.New(" No `cue ` chunk found");
        public static readonly Error InvalidSampleInformation = Error.New("Cannot determine total number of samples because either channel count or bits per sample were 0");

        public enum Format : ushort
        {
            Pcm = 1,

            Float = 3,

            Extensible = 0xFFFE,
        }

        /// <summary>
        /// Finds a RIFF header if present and returns the range of the RIFF chunk if found.
        /// </summary>
        /// <param name="stream">The file to read.</param>
        /// <returns>
        /// The range of the RIFF chunk if found, otherwise an error.
        /// The range offsets are relative to the start of the stream.
        /// </returns>
        public static Fin<RangeHelper.Range> FindRiffChunk(Stream stream)
        {
            if (stream.Length < MinimumRiffHeaderLength)
            {
                return FileTooShortRiff;
            }

            stream.Position = 0;
            Span<byte> buffer = stackalloc byte[MinimumRiffHeaderLength];
            var offset = stream.Read(buffer);

            if (offset != MinimumRiffHeaderLength)
            {
                return FileTooShortRiff;
            }

            if (!buffer[..4].SequenceEqual(RiffMagicNumber))
            {
                return InvalidFileData;
            }

            var length = BinaryPrimitives.ReadInt32LittleEndian(buffer[4..]);

            var outOfBounds = length > stream.Length;

            return new RangeHelper.Range(offset, offset + length, outOfBounds);
        }

        public static Fin<RangeHelper.Range> FindWaveChunk(Stream stream, RangeHelper.Range riffChunk)
        {
            if (riffChunk.Length < WaveMagicNumber.Length)
            {
                return FileTooShortRiff;
            }

            var offset = riffChunk.Start;
            var newOffset = stream.Seek(offset, SeekOrigin.Begin);
            if (newOffset != offset)
            {
                return InvalidOffset;
            }

            // read the first chunk type
            Span<byte> buffer = stackalloc byte[WaveMagicNumber.Length];

            var read = stream.Read(buffer);

            if (read != WaveMagicNumber.Length)
            {
                return FileTooShortRiff;
            }

            // advance our offset counter by the 4 bytes we just read
            offset += read;

            // check whether we found our target chunk or not
            if (!WaveMagicNumber.AsSpan().SequenceEqual(buffer))
            {
                // cannot process a non wave file
                return Error.New("Cannot process a non-WAVE RIFF file.");
            }

            return new RangeHelper.Range(offset, riffChunk.End);
        }

        public static Fin<RangeHelper.Range> FindFormatChunk(Stream stream, RangeHelper.Range waveChunk, bool allowOutOfBounds = false)
        {
            return ScanForChunks(stream, waveChunk, FormatChunkId, allowOutOfBounds).Map(x => x.First());
        }

        public static Fin<RangeHelper.Range> FindDataChunk(Stream stream, RangeHelper.Range waveChunk, bool allowOutOfBounds = false)
        {
            return ScanForChunks(stream, waveChunk, DataChunkId, allowOutOfBounds).Map(x => x.First());
        }

        public static Fin<RangeHelper.Range> FindCueChunk(Stream stream, RangeHelper.Range waveChunk)
        {
            return ScanForChunks(stream, waveChunk, CueChunkId, false).Map(x => x.First());
        }

        public static Fin<Seq<RangeHelper.Range>> FindListChunks(Stream stream, RangeHelper.Range waveChunk)
        {
            return ScanForChunks(stream, waveChunk, ListChunkId, false, limit: int.MaxValue);
        }

        public static Fin<Option<byte[]>> ReadListChunk(Stream stream, RangeHelper.Range waveChunk, byte[] chunkType)
        {
            return ScanForChunks(stream, waveChunk, ListChunkId, false, limit: int.MaxValue)
                .Map(chunks => FilterListChunks(stream, chunks, chunkType));
        }

        public static Fin<bool> IsWaveFile(Stream stream)
        {
            var riffChunk = FindRiffChunk(stream);

            var waveChunk = riffChunk.Bind(r => FindWaveChunk(stream, r));

            return waveChunk.IsSucc;
        }

        public static Fin<bool> IsPcmWaveFile(Stream stream)
        {
            var riffChunk = FindRiffChunk(stream);

            var waveChunk = riffChunk.Bind(r => FindWaveChunk(stream, r));

            var formatChunk = waveChunk.Bind(w => FindFormatChunk(stream, w));
            if (formatChunk.IsFail)
            {
                return (Error)formatChunk;
            }

            var formatSpan = RangeHelper.ReadRange(stream, (RangeHelper.Range)formatChunk);

            var format = GetAudioFormat(formatSpan);

            return format == Format.Pcm;
        }

        public static uint GetSampleRate(ReadOnlySpan<byte> formatChunk)
        {
            const int sampleRateOffset = 4;
            uint sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(formatChunk[sampleRateOffset..]);

            return sampleRate;
        }

        public static Format GetAudioFormat(ReadOnlySpan<byte> formatChunk)
        {
            const int audioFormatOffset = 0;
            ushort audioFormat = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[audioFormatOffset..]);

            return (Format)audioFormat;
        }

        public static ushort GetChannels(ReadOnlySpan<byte> formatChunk)
        {
            const int channelsOffset = 2;
            ushort channels = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[channelsOffset..]);

            return channels;
        }

        public static uint GetByteRate(ReadOnlySpan<byte> formatChunk)
        {
            const int byteRateOffset = 8;
            uint byteRate = BinaryPrimitives.ReadUInt32LittleEndian(formatChunk[byteRateOffset..]);

            return byteRate;
        }

        public static ushort GetBlockAlign(ReadOnlySpan<byte> formatChunk)
        {
            const int blockAlignOffset = 12;
            ushort blockAlign = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[blockAlignOffset..]);

            return blockAlign;
        }

        public static ushort GetBitsPerSample(ReadOnlySpan<byte> formatChunk)
        {
            const int bitsPerSampleOffset = 14;
            ushort bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(formatChunk[bitsPerSampleOffset..]);

            return bitsPerSample;
        }

        public static Fin<ulong> GetTotalSamples(RangeHelper.Range dataChunk, ushort channels, ushort bitsPerSample)
        {
            if (channels == 0 || bitsPerSample == 0)
            {
                return InvalidSampleInformation;
            }

            // size of the data chunk
            var length = (ulong)dataChunk.Length;

            return length / (ulong)(channels * (bitsPerSample / 8));
        }

        public static Fin<IReadOnlyCollection<Cue>> FindAndParseCuePoints(Stream stream, RangeHelper.Range waveChunk)
        {
            var cueChunk = FindCueChunk(stream, waveChunk);

            var listChunk = ReadListChunk(stream, waveChunk, AssociatedDataListChunkId);

            return cueChunk.Map(c => ParseCueChunk(stream, c, listChunk));
        }

        public static IReadOnlyCollection<Cue> ParseCueChunk(Stream stream, RangeHelper.Range cueRange, Fin<Option<byte[]>> list)
        {
            ReadOnlySpan<byte> data = RangeHelper.ReadRange(stream, cueRange);
            uint cueCount = BinaryPrimitives.ReadUInt32LittleEndian(data);
            Debug.Assert((cueCount * Marshal.SizeOf<CuePoint>()) + sizeof(uint) == data.Length, "The cue count should be correct");

            var cues = MemoryMarshal.Cast<byte, CuePoint>(data[sizeof(uint)..]);

            var notes = list
                .Map(fin =>
                    fin.Map(option => ParseAssociatedDataList(option)))
                .IfFail(None)
                .IfNone(new AssociatedDataList(new List<ICueWithText>()));

            // now associate everthing
            var result = new Cue[cues.Length];
            for (int i = 0; i < cues.Length; i++)
            {
                var cue = cues[i];

                var matching = notes.Entries.Where(note => note.CuePointId == cue.ID);
                var label = matching.FirstOrDefault(x => x is LabelChunk)?.Text;
                var note = matching.FirstOrDefault(x => x is NoteChunk)?.Text;
                var text = matching.FirstOrDefault(x => x is LabelledTextChunk)?.Text;

                result[i] = new Cue(cue.SampleOffset, label, note, text);
            }

            return result.ToArray();
        }

        public static Fin<List> ParseListChunk(ReadOnlySpan<byte> bytes)
        {
            var listType = bytes[0..ChunkIdLength];
            return listType switch
            {
                _ when listType.SequenceEqual(AssociatedDataListChunkId) => (List)ParseAssociatedDataList(bytes),
                _ when listType.SequenceEqual(InfoChunkId) => (List)ParseInfoList(bytes),
                _ => UnsupportedListType(listType),
            };
        }

        public static Option<byte[]> FilterListChunks(Stream stream, Seq<RangeHelper.Range> ranges, byte[] targetType)
        {
            foreach (var range in ranges)
            {
                var data = RangeHelper.ReadRange(stream, range);
                if (data[0..ChunkIdLength].SequenceEqual(targetType))
                {
                    return data;
                }
            }

            return Option<byte[]>.None;
        }

        /// <summary>
        /// Scans a container (a range of bytes) for a sub-chunk with the given chunk ID.
        /// The target chunk may be in any position within it's siblings.
        /// Best case: only 8 bytes are ready from the stream (i.e. the target chunk is the first in the container).
        /// Worst case: numSubChunks * 8 bytes are read from the stream (i.e. the target chunk is at the last one in the container).
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="container">The subset of the stream to read from.</param>
        /// <param name="targetChunkId">The target chunk to look for.</param>
        /// <param name="allowOutOfBounds">Set to <c>true</c> if you want out of bounds ranges to be returned. Normally the method will return Errors in out of bounds cases.</param>
        /// <param name="limit">The number of chunks to return.</param>
        /// <returns>An error if the chunk was not found, or a Range of the target chunk if it was found.</returns>
        public static Fin<Seq<RangeHelper.Range>> ScanForChunks(
            Stream stream,
            RangeHelper.Range container,
            ReadOnlySpan<byte> targetChunkId,
            bool allowOutOfBounds,
            int limit = 1)
        {
            const int ChunkLengthLength = 4;

            /*
            Check to ensure the given container range fits within the bounds of the file.
            Commented out for now since this check can cause files affected by FL005 to crash.
            https://github.com/ecoacoustics/known-problems/blob/main/frontier_labs/FL005.md

            if (container.End > stream.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(container), "container.End must be less than or equal to stream.Length");
            }
            */

            // check if the container is long enough to contain the chunk
            if (stream.Length < (ChunkIdLength + ChunkLengthLength + container.Start))
            {
                return FileTooShort(targetChunkId);
            }

            var offset = container.Start;
            Span<byte> buffer = stackalloc byte[ChunkIdLength + ChunkLengthLength];

            Seq<RangeHelper.Range> ranges = default;

            while (offset < container.End)
            {
                // seek to the start of the nth child in the container
                var newOffset = stream.Seek(offset, SeekOrigin.Begin);
                if (newOffset != offset)
                {
                    return InvalidOffset;
                }

                // read the chunk id and it's size
                var read = stream.Read(buffer);

                if (read != (ChunkIdLength + ChunkLengthLength))
                {
                    return ChunkNotFound(targetChunkId);
                }

                var chunkId = buffer[..ChunkIdLength];
                var length = BinaryPrimitives.ReadUInt32LittleEndian(buffer[ChunkIdLength..]);

                // advance our offset counter by the 8 bytes we just read
                offset += read;

                // check the chunk length falls within the bounds of the file
                bool outOfBounds = false;
                if (offset + length > stream.Length)
                {
                    // FL005 for example would trigger this branch
                    if ((offset + length) > stream.Length)
                    {
                        outOfBounds = true;
                    }

                    if (!allowOutOfBounds)
                    {
                        return InvalidChunk;
                    }
                }

                // check whether we found our target chunk or not
                if (targetChunkId.SequenceEqual(chunkId))
                {
                    // success, add the range of the chunk
                    var range = new RangeHelper.Range(offset, offset + length, outOfBounds);
                    ranges = ranges.Add(range);
                    if (ranges.Count >= limit)
                    {
                        // and stop if we've hit our limit
                        break;
                    }
                }

                // advance our offset counter by the length of the chunk to look for the next sibling
                offset += length;
            }

            if (ranges.Count > 0)
            {
                return ranges;
            }

            return ChunkNotFound(targetChunkId);
        }

        internal static Error FileTooShort(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: file is not long enough to have a {Encoding.ASCII.GetString(chunkName)} header");

        internal static Error ChunkNotFound(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Error reading file: a {Encoding.ASCII.GetString(chunkName)} chunk was not found");

        internal static Error UnsupportedListType(ReadOnlySpan<byte> chunkName) =>
            Error.New($"Cannot parse type of list {Encoding.ASCII.GetString(chunkName)}");

        private static string ParseNullTerminatedString(ReadOnlySpan<byte> bytes)
        {
            return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
        }

        private static AssociatedDataList ParseAssociatedDataList(ReadOnlySpan<byte> bytes)
        {
            Debug.Assert(bytes[0..ChunkIdLength].SequenceEqual(AssociatedDataListChunkId), "List type should always be `adtl`");

            int index = ChunkIdLength;
            var result = new List<ICueWithText>();
            do
            {
                var chunkId = bytes[index..(index + ChunkIdLength)];
                index += ChunkIdLength;
                var length = BinaryPrimitives.ReadUInt32LittleEndian(bytes[index..]);
                index += sizeof(uint);

                if (chunkId.SequenceEqual(LabelChunkId))
                {
                    var cuePointId = BinaryPrimitives.ReadUInt32LittleEndian(bytes[index..]);
                    index += sizeof(uint);
                    var textRun = length - sizeof(uint);
                    var text = ParseNullTerminatedString(bytes[index..(index + (int)textRun)]);
                    result.Add(new LabelChunk(cuePointId, text));

                    index += (int)textRun;
                }
                else if (chunkId.SequenceEqual(NoteChunkId))
                {
                    var cuePointId = BinaryPrimitives.ReadUInt32LittleEndian(bytes[index..]);
                    index += sizeof(uint);
                    var textRun = length - sizeof(uint);
                    var text = ParseNullTerminatedString(bytes[index..(index + (int)textRun)]);
                    result.Add(new NoteChunk(cuePointId, text));

                    index += (int)textRun;
                }
                else if (chunkId.SequenceEqual(LabelledTextChunkId))
                {
                    var chunk = MemoryMarshal.Read<LabelledTextChunk>(bytes[index..(int)length]);

                    result.Add(chunk);
                    index += (int)length;
                }
                else
                {
                    // unsupported chunk
                }
            }
            while (index < bytes.Length);

            return new(result);
        }

        private static Fin<InfoList> ParseInfoList(ReadOnlySpan<byte> bytes)
        {
            Debug.Assert(bytes[0..ChunkIdLength].SequenceEqual(InfoChunkId), "List type should always be `INFO`");

            var result = new List<ListItem>();

            // offset the by `INFO` 4 bytes
            int index = ChunkIdLength;
            do
            {
                var infoType = bytes[index..(index + ChunkIdLength)];
                index += ChunkIdLength;

                var size = BinaryPrimitives.ReadUInt16LittleEndian(bytes[index..]);
                index += sizeof(uint);

                var text = ParseNullTerminatedString(bytes[index..(index + size)]);
                result.Add(new ListItem(infoType.ToArray(), text));

                index += size;
            }
            while (index < bytes.Length);

            return new InfoList(result);
        }
    }
}
