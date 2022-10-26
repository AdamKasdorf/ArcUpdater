using ArcUpdater.CommandLine;

using System;
using System.Net.Http;

namespace ArcUpdater
{
    internal class Program
    {
        [STAThread]
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
            using HttpClient client = new HttpClient();
            using AssemblyUpdater updater = new AssemblyUpdater(client);
            AssemblyVerifier verifier = new AssemblyVerifier(client);

            UpdateOperation updateOperation = new UpdateOperation(verifier, updater, false);
            TargetPathOperation targetOperation = new TargetPathOperation(updateOperation, FileHelper.GetValidFilePaths);

            bool success = targetOperation.Execute(TargetPath.CurrentDirectory);
            return ErrorCode.GetValue(success); 
        }
    }
}