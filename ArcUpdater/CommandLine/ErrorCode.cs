using System;

namespace ArcUpdater.CommandLine
{
    internal static class ErrorCode
    {
        public const int Success = 0;
        public const int Failure = 1;

        public static int GetValue(bool success)
        {
            return success ? Success : Failure;
        }
    }
}
