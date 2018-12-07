// <copyright file="FixtureModel.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Tests.TestHelpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using CsvHelper.Configuration.Attributes;
    using NodaTime;

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "These fields are set by reflection when deserialized")]
    public class FixtureModel
    {
        public const string ShortFile = "Short error file";

        private string fixturePath;

        public string Name { get; private set; }

        public string FixturePath
        {
            get => this.fixturePath;

            private set
            {
                this.fixturePath = value;
                this.AbsoluteFixturePath = FixtureHelper.ResolvePath(value);
            }
        }

        [Ignore]
        public string AbsoluteFixturePath { get; private set; }

        public string Notes { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
