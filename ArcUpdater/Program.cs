using ArcUpdater.CommandLine;

namespace ArcUpdater
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                // In absence of any optional command line arguments,
                // performs standard update to current working directory.
                return Update();
            }

            using (CommandLineInterpreter interpreter = new CommandLineInterpreter())
            {
                return interpreter.Invoke(args);
            }
        }

        private static int Update()
        {
            DownloadClient downloadClient = new DownloadClient();
            AssemblyVerifier verifier = new AssemblyVerifier(downloadClient);
            AssemblyUpdater updater = new AssemblyUpdater(downloadClient);
            UpdateOperation updateOperation = new UpdateOperation(verifier, updater, false);
            TargetPathOperation targetOperation = new TargetPathOperation(updateOperation, FileHelper.GetValidFilePaths);
            bool success;

            try { success = targetOperation.Execute(TargetPath.CurrentDirectory); }
            catch { success = false; }

            return ErrorCode.GetValue(success);
        }
    }
}