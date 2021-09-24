// <copyright file="Processor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.IO;
    using System.Threading.Tasks;
    using MetadataUtility.Filenames;
    using MetadataUtility.Models;
    using MetadataUtility.Utilities;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Processes each audio recording.
    /// Has metadata extraction methods, as well as transforms, and deep data quality checks.
    /// </summary>
    public class Processor
    {
        private readonly ILogger<Processor> logger;
        private readonly FilenameParser filenameParser;
        private readonly OutputRecordWriter writer;

        private readonly FilenameSuggester filenameSuggester;

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor"/> class.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="filenameParser">A filename parser.</param>
        /// <param name="writer">The sink to send the output.</param>
        /// <param name="filenameSuggester">An instance of a filename suggester.</param>
        public Processor(ILogger<Processor> logger, FilenameParser filenameParser, OutputRecordWriter writer, FilenameSuggester filenameSuggester)
        {
            this.logger = logger;
            this.filenameParser = filenameParser;
            this.writer = writer;
            this.filenameSuggester = filenameSuggester;
        }

        /// <summary>
        /// Processes a single recording.
        /// </summary>
        /// <param name="path">The path to the file to process.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Recording> ProcessFile(string path)
        {
            this.logger.LogInformation("Processing file {path}", path);

            await Task.Yield();

            var recording = new Recording
            {
                SourcePath = path,
                Stem = Path.GetFileNameWithoutExtension(path),
            };

            // step 0. validate
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException($"Could not find the file {file}");
            }

            // step 1. parse filename
            var parsedName = this.filenameParser.Parse(file.Name);
            recording.Extension = parsedName.Extension;

            this.ResolveDateTime(recording, parsedName);

            recording.Location = parsedName.Location;

            recording.RecommendedName = this.filenameSuggester.SuggestName(recording, parsedName, null);

            //recording.StartDate = MetadataSource<OffsetDateTime>.Provenance.Calculated.Wrap<OffsetDateTime>(parsedName.OffsetDateTime.Value);
            this.logger.LogDebug("Parsed filename: {@name}", parsedName);

            this.logger.LogDebug("Completed file {path}", path);
            return await Task.FromResult(recording);
        }

        /// <summary>
        /// Performs deep checks of the target recording.
        /// </summary>
        /// <remarks>
        /// Deep checks are checks that analyze all the frames or bytes of a given file.
        /// </remarks>
        /// <param name="recording">The recording to analyze.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<Recording> DeepCheck(Recording recording)
        {
            await Task.Yield();

            return recording;
        }

        //        /// <summary>
        //        /// Writes the recording metadata out to a sink.
        //        /// </summary>
        //        /// <param name="recording">The recording to write.</param>
        //        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        //        public async Task<Recording> Write(Recording recording)
        //        {
        //            void Write()
        //            {
        //                lock (this.writer)
        //                {
        //                    this.writer.Write(recording);
        //                }
        //            }
        //
        //            await Task.Run(Write);
        //            return recording;
        //        }

        //        /// <summary>
        //        /// Runs through all the process steps.
        //        /// </summary>
        //        /// <param name="path">The path to the recording to process.</param>
        //        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        //        public async Task<Recording> All(string path)
        //        {
        //            var recording = await this.ProcessFile(path);
        //            recording = await this.Write(recording);
        //
        //            return recording;
        //
        //
        //        }

        private void ResolveDateTime(Recording recording, ParsedFilename filename)
        {
            if (filename.OffsetDateTime.HasValue)
            {
                recording.StartDate = filename.OffsetDateTime.Value.SourcedFrom(Provenance.Filename);
                this.logger.LogTrace("Full date parsed from filename {path}", recording.SourcePath);
            }
            else if (filename.LocalDateTime.HasValue)
            {
                throw new NotImplementedException();
                //if (this.arguments.UtcOffsetHint.HasValue)
                //{
                //    recording.StartDate = filename
                //        .LocalDateTime
                //        .Value
                //        .WithOffset(this.arguments.UtcOffsetHint.Value)
                //        .SourcedFrom(Provenance.Filename | Provenance.UserSupplied);

                //    this.logger.LogTrace("Full date parsed from filename {0} and UTC offset hint used", recording.SourcePath);
                //}
                //else
                //{
                //    this.logger.LogWarning("Could not unambiguously parse date for {0}", recording.SourcePath);
                //    recording.Errors.Add(new Error(WellKnownProblems.AmbiguousDate));
                //}
            }
            else
            {
                recording.Errors.Add(new Error(WellKnownProblems.NoDateFound));
            }
        }
    }
}
