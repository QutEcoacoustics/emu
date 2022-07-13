// <copyright file="IEnumerableExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group.
// </copyright>

namespace Emu.Extensions.System
{
    public static class IEnumerableExtensions
    {
        public static async Task<IEnumerable<T>> WaitAllAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }
    }
}
