namespace MetadataUtility.Cli
{
    public static class ExitCodes
    {
        public const int Success = 0;
        public const int Failure = 1;

        public static int Get(bool success)
        {
            return success ? Success : Failure;
        }
    }
}
