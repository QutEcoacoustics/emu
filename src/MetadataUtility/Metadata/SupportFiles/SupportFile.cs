// <copyright file="SupportFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles
{
    public abstract class SupportFile
    {
        public string File { get; set; }

        public static List<string> FindSupportFiles(TargetInformation information, string pattern)
        {
            List<string> logFiles = new List<string>();
            string fileDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);
            string searchDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);

            int i = 0;

            while (i++ < 3 && (logFiles = information.FileSystem.Directory.GetFiles(searchDirectory, pattern, SearchOption.AllDirectories).ToList()).Length() == 0)
            {
                searchDirectory = information.FileSystem.Directory.GetParent(searchDirectory)?.FullName;

                //return if root directory is reached before any log files are found
                if (searchDirectory == null)
                {
                    return logFiles;
                }
            }

            if (logFiles.Length() == 1)
            {
                string logDirectory = information.FileSystem.Path.GetDirectoryName(logFiles[0]);

                // If the log file is in a subdirectory of the audio file or vice versa, they are linked
                // Otherwise we can't assume they are!
                if (searchDirectory.Equals(fileDirectory) ||
                    information.FileSystem.Directory.GetFiles(logDirectory, "*", SearchOption.AllDirectories).ToList().Contains(information.Path))
                {
                    return logFiles;
                }
                else
                {
                    logFiles.RemoveAt(0);
                    return logFiles;
                }
            }

            return logFiles;
        }

        /// <summary>
        /// Extracts all useful information out of the log file.
        /// This should done only once for each log file, the result should then be cached in KnownSupportFiles (TargetInformation.cs).
        /// </summary>
        public abstract void ExtractInformation();
    }
}
