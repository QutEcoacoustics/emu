// <copyright file="SupportFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles
{
    using System;

    public abstract class SupportFile
    {
        // Each of these functions are used to correlate a specific type of support file to targets
        public static Action<TargetInformation, IEnumerable<string>>[] SupportFileFinders
        {
            get
            {
                return new Action<TargetInformation, IEnumerable<string>>[]
                {
                    FrontierLabs.LogFile.FindLogFile,
                };
            }
        }

        // Each potential support file pattern to search for
        public static string[] SupportFilePatterns
        {
            get
            {
                return new string[]
                {
                    "*logfile*.txt",
                };
            }
        }

        public string FilePath { get; set; }

        public static void FindSupportFiles(TargetInformation information)
        {
            List<string> supportFiles = new List<string>();
            string fileDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);
            string searchDirectory = fileDirectory;

            int i = 0;
            const int maxHeight = 3;

            while (i++ < maxHeight)
            {
                // Find any potential support files
                foreach (string pattern in SupportFilePatterns)
                {
                    supportFiles.AddRange(information.FileSystem.Directory.GetFiles(searchDirectory, pattern, SearchOption.TopDirectoryOnly));
                }

                // We assume that support files will only be found in one directory!
                if (supportFiles.Any())
                {
                    break;
                }

                searchDirectory = information.FileSystem.Directory.GetParent(searchDirectory)?.FullName;

                //return if root directory is reached before any support files are found
                if (searchDirectory == null)
                {
                    return;
                }
            }

            // Correlate specific support files to each target
            foreach (Action<TargetInformation, IEnumerable<string>> finder in SupportFileFinders)
            {
                finder(information, supportFiles);
            }
        }

        /// <summary>
        /// Extracts all useful information out of the support file.
        /// This should done only once for each support file, the result should then be cached in KnownSupportFiles (TargetInformation.cs).
        /// </summary>
        public abstract void ExtractInformation();
    }
}
