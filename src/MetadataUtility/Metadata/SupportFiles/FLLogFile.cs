// <copyright file="FLLogFile.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata.SupportFiles
{
    using System.Text.RegularExpressions;
    using LanguageExt;

    public static class FLLogFile
    {
        public const string FrontierLabsLogString = " FRONTIER LABS Bioacoustic Audio Recorder ";

        public static Fin<bool> FileExists(TargetInformation information)
        {
            List<string> logFiles = new List<string>();

            // If support file directories were given, search there for log files, if not search the directory of the audio file
            if (information.SupportFileDirectories == null)
            {
                logFiles = information.FileSystem.Directory.GetFiles(information.FileSystem.Path.GetDirectoryName(information.Path), "*logfile*.txt", SearchOption.AllDirectories).ToList();
            }
            else
            {
                foreach (string dir in information.SupportFileDirectories)
                {
                    logFiles.AddRange(information.FileSystem.Directory.GetFiles(dir, "*logfile*.txt", SearchOption.AllDirectories));
                }
            }

            foreach (string logFile in logFiles)
            {
                string[] lines = information.FileSystem.File.ReadAllLines(logFile);

                if (lines[2].Equals(FrontierLabsLogString))
                {
                    // Newer firmware versions contain the file name in the log file, search for that first
                    foreach (string line in lines)
                    {
                        if (line.Contains(information.FileSystem.Path.GetFileName(information.Path)))
                        {
                            information.KnownSupportFiles.Add("Log file", logFile);
                            return true;
                        }
                    }

                    // Check if the log file is the only log file in its directory (and all subdirectories), and if the audio file is in its directory (or a subdirectory)
                    List<string> files = information.FileSystem.Directory.GetFiles(information.FileSystem.Path.GetDirectoryName(logFile), "*", SearchOption.AllDirectories).ToList();

                    if (files.Count(s => Regex.IsMatch(s, ".*logfile.*txt")) == 1 && files.Count(s => s.EndsWith(information.FileSystem.Path.GetFileName(information.Path))) == 1)
                    {
                        information.KnownSupportFiles.Add("Log file", logFile);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
