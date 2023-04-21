// <copyright file="NodatimeSerializersTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Tests.Serialization
{
    using System;
    using System.Globalization;
    using System.IO;
    using CsvHelper.TypeConversion;
    using Emu.Serialization;
    using NodaTime;
    using Xunit;

    public class NodatimeSerializersTests
    {
        [Fact]
        public void OffsetConverter()
        {
            var value = Offset.FromHoursAndMinutes(5, 30);
            string json = "+05:30";
            this.AssertCsvSerialization(value, json, NodatimeConverters.OffsetConverter);
        }

        [Fact]
        public void OffsetDateTimeConverter()
        {
            var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.FromHoursAndMinutes(-1, -30));
            string text = "2012-01-02T03:04:05.123456789-01:30";
            this.AssertCsvSerialization(value, text, NodatimeConverters.OffsetDateTimeConverter);
        }

        [Fact]
        public void OffsetDateTimeConverter_WholeHours()
        {
            // Redundantly specify the minutes, so that Javascript can parse it and it's RFC3339-compliant.
            // See issue 284 for details.
            var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.FromHours(5));
            string text = "2012-01-02T03:04:05.123456789+05:00";
            this.AssertCsvSerialization(value, text, NodatimeConverters.OffsetDateTimeConverter);
        }

        [Fact]
        public void LocalDateTimeConverter()
        {
            var value = new LocalDateTime(2012, 1, 2, 3, 4, 5, CalendarSystem.Iso).PlusNanoseconds(123456789);
            var text = "2012-01-02T03:04:05.123456789";
            this.AssertCsvSerialization(value, text, NodatimeConverters.LocalDateTimeConverter);
        }

        [Fact]
        public void LocalDateTimeConverter_SerializeNonIso_Throws()
        {
            var localDateTime = new LocalDateTime(2012, 1, 2, 3, 4, 5, CalendarSystem.Coptic);

            Assert.Throws<ArgumentException>(() => this.AssertCsvSerialization(localDateTime, null, NodatimeConverters.LocalDateTimeConverter));
        }

        [Fact]
        public void OffsetDateTimeConverter_ZeroOffset()
        {
            // Redundantly specify the minutes, so that Javascript can parse it and it's RFC3339-compliant.
            // See issue 284 for details.
            var value = new LocalDateTime(2012, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).WithOffset(Offset.Zero);
            string text = "2012-01-02T03:04:05.123456789Z";
            this.AssertCsvSerialization(value, text, NodatimeConverters.OffsetDateTimeConverter);
        }

        [Fact]
        public void Duration_WholeSeconds()
        {
            this.AssertCsvSerialization(Duration.FromHours(48), "172800", NodatimeConverters.DurationConverter);
        }

        [Fact]
        public void Duration_FractionalSeconds()
        {
            var d48 = Duration.FromHours(48) + Duration.FromSeconds(3);

            this.AssertCsvSerialization(d48 + Duration.FromNanoseconds(123456789), "172803.123456789", NodatimeConverters.DurationConverter);
            this.AssertCsvSerialization(d48 + Duration.FromTicks(1230000), "172803.123", NodatimeConverters.DurationConverter);
            this.AssertCsvSerialization(d48 + Duration.FromTicks(1234000), "172803.1234", NodatimeConverters.DurationConverter);
            this.AssertCsvSerialization(d48 + Duration.FromTicks(12345), "172803.0012345", NodatimeConverters.DurationConverter);
        }

        [Fact]
        public void Duration_MinAndMaxValues()
        {
            this.AssertCsvSerialization(Duration.FromTicks(long.MaxValue), "922337203685.4775807", NodatimeConverters.DurationConverter);
            this.AssertCsvSerialization(Duration.FromTicks(long.MinValue), "-922337203685.4775808", NodatimeConverters.DurationConverter);
        }

        private void AssertCsvSerialization<T>(T value, string expected, ITypeConverter converter)
        {
            string actual;
            using (var writer = new StringWriter())
            {
                var serializer = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);
                serializer.Context.TypeConverterCache.AddConverter<T>(converter);
                serializer.WriteField(value);
                serializer.Flush();

                actual = writer.ToString();
                Assert.Equal(expected, actual);
            }

            var reader = new CsvHelper.CsvReader(new StringReader(actual), CultureInfo.InvariantCulture);
            reader.Context.TypeConverterCache.AddConverter<T>(converter);
            reader.Read();
            var deserializedValue = reader.GetField<T>(0);
            Assert.Equal(value, deserializedValue);
        }
    }
}
