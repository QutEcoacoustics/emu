// <copyright file="Processor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using MetadataUtility.Filenames;
    using Microsoft.Extensions.Logging;

    public class Processor
    {
        private readonly ILogger<Processor> logger;
        private readonly FilenameParser filenameParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor"/> class.
        /// </summary>
        /// <param name="logger">A logger.</param>
        /// <param name="filenameParser">A filename parser.</param>
        public Processor(ILogger<Processor> logger, FilenameParser filenameParser)
        {
            this.logger = logger;
            this.filenameParser = filenameParser;
        }

        public async Task<bool> ProcessFile(string path)
        {
            this.logger.LogInformation("Processing file {0}", path);

            await Task.Yield();

            // step 0. validate
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException($"Could not find the file {file}");
            }

            // step 1. parse filename
            var parsedName = this.filenameParser.Parse(file.Name);
            this.logger.LogDebug("Parsed filename: {@0}", parsedName);

            this.logger.LogDebug("Completed file {0}", path);
            return await Task.FromResult(true);
        }
    }
}
