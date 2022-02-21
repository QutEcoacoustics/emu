// <copyright file="MetadataRegister.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Metadata
{
    using MetadataUtility.Metadata.FrontierLabs;

    public class MetadataRegister
    {
        public static readonly IReadOnlyCollection<Type> KnownOperations = new[]
        {
            // each time we make a new extractor we'll add it here
            typeof(FilenameExtractor),
            typeof(WaveHeaderExtractor),
        };

        private readonly IServiceProvider provider;

        private IEnumerable<IMetadataOperation> resolved;

        public MetadataRegister(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IEnumerable<IMetadataOperation> All
        {
            get
            {
                this.resolved ??= KnownOperations.Select(x => (IMetadataOperation)this.provider.GetService(x));
                return this.resolved;
            }
        }
    }
}
