namespace MetadataUtility.Tests.TestHelpers
{
    using System.IO.Abstractions.TestingHelpers;

    public static class Extensions
    {
        public static void AddEmptyFile(this MockFileSystem fileSystem, string path)
        {
            fileSystem.AddFile(path, string.Empty);
        }
    }
}
