// <copyright file="Version.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Commands.Version
{
    using System.CommandLine.Invocation;
    using MetadataUtility.Utilities;
    using static ThisAssembly;
    using static ThisAssembly.Git;

    public class Version : EmuCommandHandler
    {
        public Version(OutputRecordWriter writer)
        {
            this.Writer = writer;
        }

        public override Task<int> InvokeAsync(InvocationContext context)
        {
            var record = new VersionRecord(
                InformationalVersion,
                RepositoryUrl,
                BaseVersion.Major,
                BaseVersion.Minor,
                Commits,
                Branch,
                Commit,
                Sha,
                CommitDate);

            this.WriteHeader<VersionRecord>();
            this.Write<VersionRecord>(record);

            return Task.FromResult(0);
        }

        protected override object FormatCompact<T>(T record)
        {
            if (record is VersionRecord v)
            {
                return v.Version;
            }

            return ThrowUnsupported(record);
        }

        protected override object FormatDefault<T>(T record)
        {
            if (record is VersionRecord v)
            {
                return v.Version;
            }

            return ThrowUnsupported(record);
        }

        private record VersionRecord(
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
