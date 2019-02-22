// <copyright file="OffsetValidator.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Cli
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.RegularExpressions;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Validation;
    using MetadataUtility.Dates;

    /// <summary>
    /// Validates a given string is an Offset that we accept.
    /// </summary>
    public class OffsetValidator : IOptionValidator
    {
        private static readonly Regex OffsetRegex = new Regex(
            "(^[-+]?[0-1][0-9]:?([0-9][0-9])?|Z)$",
            RegexOptions.Compiled);

        /// <inheritdoc />
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (!option.HasValue())
            {
                return ValidationResult.Success;
            }

            if (OffsetRegex.IsMatch(option.Value()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Could not parse UTC offset");
        }
    }
}
