// <copyright file="AudioMothDateParser.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Dates
{
    using System;
    using System.Globalization;
    using System.Text;
    using Emu.Globalization;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// A NodaTime <see cref="IPattern{T}"/> implementation for AudioMoth dates.
    /// </summary>
    public class AudioMothDateParser : IPattern<OffsetDateTime>
    {
        private static readonly Instant Epoch = NodaConstants.UnixEpoch;

        /// <inheritdoc />
        public ParseResult<OffsetDateTime> Parse(string text)
        {
            var successful = long.TryParse(
                text,
                NumberStyles.AllowHexSpecifier,
                CultureInfo.InvariantCulture,
                out var secondsSinceEpoch);

            if (successful)
            {
                var trueValue = Epoch + Duration.FromSeconds(secondsSinceEpoch);
                return ParseResult<OffsetDateTime>.ForValue(trueValue.WithOffset(Offset.Zero));
            }

            return ParseResult<OffsetDateTime>.ForException(FailParsing);

            Exception FailParsing()
            {
                return new UnparsableValueException(Strings.AudioMothDateParserParseFailure.Template(text));
            }
        }

        /// <inheritdoc />
        public string Format(OffsetDateTime value)
        {
            var instant = value.ToInstant();
            if (instant < Epoch)
            {
                throw new InvalidOperationException(Strings.AudioMothDateFormatFailure.Template(value));
            }

            var delta = instant - Epoch;
            return ((int)Math.Round(delta.TotalSeconds, MidpointRounding.AwayFromZero)).ToString("X");
        }

        /// <inheritdoc/>
        public StringBuilder AppendFormat(OffsetDateTime value, StringBuilder builder) =>
            builder.Append(this.Format(value));
    }
}
