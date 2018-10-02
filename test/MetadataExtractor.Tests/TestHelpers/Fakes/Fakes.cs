// <copyright file="Fakes.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataExtractor.Tests.TestHelpers.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Bogus;
    using MetadataExtractor.Models;
    using Xunit;

    public class Fakes : IDisposable
    {
        private static readonly string[] KnownHashes = new[] { "SHA-2-256", "SHA-2-512" };

        public Faker<Recording> GetRecording()
        {
            return new Faker<Recording>()
                .RuleFor(x => x.CalculatedChecksum, () => this.GetChecksum());
        }

        public Faker<Checksum> GetChecksum()
        {
            return new Faker<Checksum>()
                .StrictMode(true)
                .RuleFor(x => x.Type, f => f.PickRandom(KnownHashes))
                .RuleFor(x => x.Value, f => f.Random.AlphaNumeric(256));
        }

        public void Dispose()
        {
        }

        [CollectionDefinition(nameof(Fakes))]
        public class DatabaseCollection : ICollectionFixture<Fakes>
        {
            // This class has no code, and is never created. Its purpose is simply
            // to be the place to apply [CollectionDefinition] and all the
            // ICollectionFixture<> interfaces.
        }

    }
}
