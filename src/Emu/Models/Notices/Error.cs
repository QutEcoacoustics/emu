// <copyright file="Error.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Models.Notices
{
    using LanguageExt;

    public partial record Error(string Message, WellKnownProblem Problem = null) : Notice(Message, Problem);

    public partial record Error
    {
        public static Error FromExpctedError(LanguageExt.Common.Error error) => error switch
        {
            LanguageExt.Common.Exceptional exceptional =>
                throw new InvalidOperationException(
                    "Cannot transform exceptional errors",
                    exceptional.ToException()),
            _ => new Error(error.Message),
        };

        public static Seq<Error> FromExpectedErrors(IEnumerable<LanguageExt.Common.Error> errors) =>
            errors.Map(FromExpctedError).ToSeq();
    }
}
