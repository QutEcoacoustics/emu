// <copyright file="OutputWriterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MetadataUtility.Tests.TestHelpers.Fakes;
    using Xunit;

    public class OutputWriterTests : IClassFixture<Fakes>
    {
        private readonly Fakes fakes;

        public OutputWriterTests(Fakes fakes)
        {
            this.fakes = fakes;
        }

        [Fact]
        public void OutputWriterCanBeConfigured()
        {
            var fake = this.fakes.GetRecording();


        }
    }
}
