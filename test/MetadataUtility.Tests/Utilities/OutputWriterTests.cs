// <copyright file="OutputWriterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using MetadataUtility.Tests.TestHelpers.Fakes;
    using MetadataUtility.Utilities;
    using Xunit;

    public class OutputWriterTests : IClassFixture<Fakes>
    {
        private readonly Fakes fakes;

        public OutputWriterTests(Fakes fakes)
        {
            this.fakes = fakes;
        }

        [Fact]
        public void OutputWriterWriteJson()
        {
            var fake = this.fakes.GetRecording();
            using (var stringWriter = new StringWriter())
            {
                var output = new OutputWriter(new MetadataUtility.Serialization.JsonSerializer(), stringWriter);


            }
        }

        [Fact]
        public void OutputWriterWriteCsv()
        {
            var fake = this.fakes.GetRecording();


        }
    }
}
