// <copyright file="RenameResult.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Filenames
{
    /// <summary>
    /// A reccord of a file rename.
    /// </summary>
    /// <param name="OldName">The old name.</param>
    /// <param name="NewName">The new name.</param>
    /// <param name="Reason">The reason new name is unchanged.</param>
    public record RenameResult(string OldName, string NewName, string Reason);
}
