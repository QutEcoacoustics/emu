// <copyright file="Errors.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace MetadataUtility.Audio
{
    using LanguageExt.Common;

    public class Errors
    {
        public static readonly Error FileTooShort = Error.New("Error reading file: file is not long enough to have a duration header");
    }
}
