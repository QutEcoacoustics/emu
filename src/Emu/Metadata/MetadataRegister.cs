// <copyright file="MetadataRegister.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Metadata
{
    using System.Diagnostics.CodeAnalysis;
    using Emu.Metadata.FrontierLabs;
    using Emu.Metadata.WildlifeAcoustics;

    public class MetadataRegister
    {
        public static readonly IEnumerable<MetadataOperation> KnownOperations = new MetadataOperation[]
        {
            // each time we make a new extractor we'll add it here
            new(typeof(FilenameExtractor)),
            new(typeof(WaveHeaderExtractor)),
            new(typeof(FlacHeaderExtractor)),
            new(typeof(FlacCommentExtractor)),
            new(typeof(WamdExtractor)),
            new(typeof(LogFileExtractor)),
            new(typeof(HashCalculator)),
            new(typeof(ExtensionInferer)),
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
                this.resolved ??= KnownOperations.Select(
                    x => (IMetadataOperation)this.provider.GetService(x.Type));
                return this.resolved;
            }
        }

        public T Get<T>()
        {
            var operation = KnownOperations.First(x => typeof(T) == x.Type);
            return (T)this.provider.GetService(operation.Type);
        }

        public record MetadataOperation(
            [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            Type Type);
    }
}
