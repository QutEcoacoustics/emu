// <copyright file="WellKnownProblem.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility
{
    using System.ComponentModel;

    [TypeConverter(typeof(WellKnownProblemTypeConverter))]
    public record WellKnownProblem(
        string Title,
        string Message,
        string Code,
        string Prefix,
        string Url)
    {
        /// <summary>
        /// Gets a short title for the notice.
        /// </summary>
        public string Title { get; init; } = Title;

        /// <summary>
        /// Gets a detailed message for the notice.
        /// </summary>
        public string Message { get; init; } = Message;

        /// <summary>
        /// Gets a unique idnetifying code for the notice.
        /// </summary>
        /// <remarks>
        /// The code is used for well known problems and allows linking to
        /// an associated problem.
        /// </remarks>
        public string Code { get; init; } = Code;

        /// <summary>
        /// Gets a prefix for the notice.
        /// </summary>
        /// <remarks>
        /// The the prefix is added to the code to allow easy grouping of similar notices.
        /// Typically notices relavant to a vendor share the same prefix.
        /// </remarks>
        public string Prefix { get; init; } = Prefix;

        /// <summary>
        /// Gets the identifier for this problem.
        /// </summary>
        public string Id => this.Prefix + this.Code;
    }
}
