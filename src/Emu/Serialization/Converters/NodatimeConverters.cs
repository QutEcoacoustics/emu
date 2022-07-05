// <copyright file="NodatimeConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Serialization
{
    /// <summary>
    /// A collection of convertes for Nodatime types for CsvHelper.
    /// </summary>
    public class NodatimeConverters
    {
        /// <summary>
        /// Gets a singleton <see cref="OffsetDateTimeConverter"/>.
        /// </summary>
        public static OffsetDateTimeConverter OffsetDateTimeConverter { get; } = new OffsetDateTimeConverter();

        /// <summary>
        /// Gets a singleton <see cref="LocalDateTimeConverter"/>.
        /// </summary>
        public static LocalDateTimeConverter LocalDateTimeConverter { get; } = new LocalDateTimeConverter();

        /// <summary>
        /// Gets a singleton <see cref="NodatimeConverters.DurationConverter"/>.
        /// </summary>
        public static DurationConverter DurationConverter { get; } = new DurationConverter();

        /// <summary>
        /// Gets a singleton <see cref="OffsetConverter"/>.
        /// </summary>
        public static OffsetConverter OffsetConverter { get; } = new OffsetConverter();

        /// <summary>
        /// Gets a singleton <see cref="InstantConverter"/>.
        /// </summary>
        public static InstantConverter InstantConverter { get; } = new InstantConverter();
    }
}
