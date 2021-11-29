// <copyright file="UtcOffsetValidator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using NodaTime;
    using NodaTime.Text;

    public static class UtcOffsetOption
    {
        private static readonly OffsetPattern OffsetPattern = OffsetPattern.GeneralInvariantWithZ;
        private static readonly OffsetPattern OffsetPattern2 = OffsetPattern.CreateWithInvariantCulture("I");

        private static readonly Regex OffsetRegex = new(
            "(^[-+]?[0-1][0-9]:?([0-9][0-9])?|Z)$",
            RegexOptions.Compiled);

        public static ParseArgument<Offset?> Parser { get; } = (argument) =>
        {
            var token = argument.Tokens.SingleOrDefault()?.Value;

            if (token == null)
            {
                argument.ErrorMessage = "More than one token";
                return default;
            }

            // allow for bare numbers without leading "+" prefix
            if (!(token.FirstOrDefault() is '+' or '-' or 'Z'))
            {
                token = "+" + token;
            }

            var result = OffsetPattern.Parse(token);
            if (result.Success)
            {
                return result.Value;
            }

            result = OffsetPattern2.Parse(token);
            if (result.Success)
            {
                return result.Value;
            }

            argument.ErrorMessage = "Unable to parse offset";
            return default;
        };

        /// <summary>
        /// Checks if a time span is a valid UTC offset. I.e. it has no seconds portion.
        /// </summary>
        /// <param name="option">The option to add the validator to.</param>
        /// <returns>The option that the validator was added to.</returns>
        public static Option<Offset?> ValidUtcOffset(this Option<Offset?> option)
        {
            option.AddValidator(IsValid);

            return option;
        }

        public static string IsValid(OptionResult symbol)
        {
            if (symbol.Token is null)
            {
                return default;
            }

            var valid = symbol.Children.SelectMany(x => x.Tokens).All(x => OffsetRegex.IsMatch(x.Value));
            if (!valid)
            {
                return "Could not parse UTC offset";
            }

            return default;
        }
    }
}
