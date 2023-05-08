// <copyright file="Version.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Commands.Version
{
    using System.CommandLine.Invocation;
    using Emu.Utilities;
    using static Emu.Cli.SpectreUtils;

    public class Version : EmuCommandHandler<Version.VersionRecord>
    {
        public Version(OutputRecordWriter writer)
        {
            this.Writer = writer;
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            // https://github.com/devlooped/GitInfo/issues/273
            var record = new VersionRecord(
                ThisAssembly.InformationalVersion,
                System.Reflection.ThisAssembly.Git.RepositoryUrl,
                System.Reflection.ThisAssembly.Git.BaseVersion.Major,
                System.Reflection.ThisAssembly.Git.BaseVersion.Minor,
                System.Reflection.ThisAssembly.Git.Commits,
                System.Reflection.ThisAssembly.Git.Branch,
                System.Reflection.ThisAssembly.Git.Commit,
                System.Reflection.ThisAssembly.Git.Sha,
                System.Reflection.ThisAssembly.Git.CommitDate);

            this.WriteHeader();
            this.Write(record);
            this.WriteFooter();

            return Task.FromResult(0);
        }

        public override string FormatCompact(Version.VersionRecord record)
        {
            return record.Version;
        }

        public override object FormatRecord(Version.VersionRecord record)
        {
            return EmuName + " " + record.Version;
        }

        public record VersionRecord(
            string Version,
            string RepositoryUrl,
            string Major,
            string Minor,
            string Commits,
            string Branch,
            string ShortHash,
            string LongHash,
            string CommitDate);
    }
}
