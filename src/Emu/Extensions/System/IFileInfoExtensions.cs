// <copyright file="IFileInfoExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace System
{
    using System.IO.Abstractions;

    public static class IFileInfoExtensions
    {
        public static IFileInfo Touch(this IFileInfo info)
        {
            ArgumentNullException.ThrowIfNull(info, nameof(info));

            info.Directory!.Create();

            info.OpenWrite().Close();

            info.Refresh();

            return info;
        }
    }
}
