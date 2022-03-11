// <copyright file="SupportFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles
{
    public abstract class SupportFile
    {
        public string FilePath { get; set; }

        public static List<string> FindSupportFiles(TargetInformation information, string pattern)
        {
            List<string> supportFiles = new List<string>();
            string fileDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);
            string searchDirectory = fileDirectory;

            int i = 0;
            const int maxHeight = 3;

            while (i++ < maxHeight)
            {
                supportFiles = information.FileSystem.Directory.GetFiles(searchDirectory, pattern, SearchOption.AllDirectories).ToList();

                if (supportFiles.Any())
                {
                    break;
                }

                searchDirectory = information.FileSystem.Directory.GetParent(searchDirectory)?.FullName;

                //return if root directory is reached before any log files are found
                if (searchDirectory == null)
                {
                    return supportFiles;
                }
            }

            if (supportFiles.Length() == 1)
            {
                string logDirectory = information.FileSystem.Path.GetDirectoryName(supportFiles[0]);

                // If the log file is in a subdirectory of the audio file or vice versa, they are linked
                // Otherwise we can't assume they are!
                if (searchDirectory.Equals(fileDirectory) ||
                    information.FileSystem.Directory.GetFiles(logDirectory, "*", SearchOption.AllDirectories).ToList().Contains(information.Path))
                {
                    return supportFiles;
                }
                else
                {
                    supportFiles.RemoveAt(0);
                    return supportFiles;
                }
            }

            return supportFiles;
        }

        /// <summary>
        /// Extracts all useful information out of the support file.
        /// This should done only once for each support file, the result should then be cached in KnownSupportFiles (TargetInformation.cs).
        /// </summary>
        public abstract void ExtractInformation();
    }
}
