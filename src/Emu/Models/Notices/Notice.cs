// <copyright file="Notice.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models.Notices
{
    /// <inheritdoc />
    public abstract partial record Notice(string Message, WellKnownProblem Problem = null);

    public abstract partial record Notice : IFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format != "G")
            {
                throw new ArgumentNullException("format", "Only the G format specifier is supported");
            }

            var category = this switch
            {
                Info => string.Empty,
                Warning => "Warning",
                Error => "Error",
                _ => throw new NotSupportedException("Unknown notice type"),
            };

            var problem = this.Problem is null ? string.Empty : $" {this.Problem.Id} ({this.Problem.Title})";
            var message = string.IsNullOrEmpty(this.Message) ? this.Problem?.Message : this.Message;

            return $"{category}{problem}: {message}";
        }
    }
}
