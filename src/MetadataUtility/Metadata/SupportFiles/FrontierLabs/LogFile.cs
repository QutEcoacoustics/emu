// <copyright file="LogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles.FrontierLabs
{
    using LanguageExt;

    public static class LogFile
    {
        public const string FrontierLabsLogString = "FRONTIER LABS Bioacoustic Audio Recorder";
        public const string LogFileKey = "Log file";

        public static Fin<bool> FileExists(TargetInformation information)
        {
            List<string> logFiles = new List<string>();
            string fileDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);
            string searchDirectory = information.FileSystem.Path.GetDirectoryName(information.Path);

            int i = 0;

            while (i++ < 3 && (logFiles = information.FileSystem.Directory.GetFiles(searchDirectory, "*logfile*.txt", SearchOption.AllDirectories).ToList()).Length() == 0)
            {
                searchDirectory = information.FileSystem.Directory.GetParent(searchDirectory)?.FullName;

                //return false if root directory is reached before any log files are found
                if (searchDirectory == null)
                {
                    return false;
                }
            }

            if (logFiles.Length() == 1)
            {
                string logDirectory = information.FileSystem.Path.GetDirectoryName(logFiles[0]);

                // If the log file is in a subdirectory of the audio file or vice versa, they are linked
                // Otherwise we can't assume they are!
                if ((searchDirectory!.Equals(fileDirectory) ||
                    information.FileSystem.Directory.GetFiles(logDirectory, "*", SearchOption.AllDirectories).ToList().Contains(information.Path)) &&
                    IsLogFile(logFiles[0]))
                {
                    information.KnownSupportFiles.Add(LogFileKey, logFiles[0]);
                    return true;
                }
            }

            foreach (string logFile in logFiles)
            {
                if (IsLogFile(logFile))
                {
                    string[] lines = information.FileSystem.File.ReadAllLines(logFile);

                    // Newer firmware versions contain the file name in the log file
                    foreach (string line in lines)
                    {
                        if (line.Contains(information.FileSystem.Path.GetFileName(information.Path)))
                        {
                            information.KnownSupportFiles.Add(LogFileKey, logFile);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsLogFile(string logFile)
        {
            using (StreamReader reader = new StreamReader(logFile))
            {
                for (int i = 0; i < 5; i++)
                {
                    if ((reader.ReadLine() ?? string.Empty).Contains(FrontierLabsLogString))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
