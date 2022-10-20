using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace ArcUpdater.CommandLine
{
    internal class CommandLineInterpreter : IDisposable
    {
        private const string CommandDesc = "Check for updates to ArcDPS, download the latest version, verify the integrity of the assembly, and install to the current working directory or specified directories and file paths.";

        private readonly RootCommand _rootCommand;
        private DownloadClient _downloadClient;
        private AssemblyVerifier _verifier;
        private AssemblyUpdater _updater;

        private bool _disposed;

        public CommandLineInterpreter()
        {
            _downloadClient = new DownloadClient();
            _verifier = new AssemblyVerifier(_downloadClient);
            _updater = new AssemblyUpdater(_downloadClient);

            VerifyCommand verify = new VerifyCommand(_verifier);
            UpdateCommand update = new UpdateCommand(_verifier, _updater);

            _rootCommand = new RootCommand(CommandDesc);
            _rootCommand.TreatUnmatchedTokensAsErrors = true;
            _rootCommand.Handler = update.Handler;
            _rootCommand.Add(new PathsArgument());
            _rootCommand.Add(new DeleteOption());
            _rootCommand.Add(new CleanCommand());
            _rootCommand.Add(verify);
            _rootCommand.Add(update);
        }

        ~CommandLineInterpreter()
        {
            _updater.Dispose();
#if !NET6_0_OR_GREATER
            _downloadClient.Dispose();
#endif
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_updater != null)
            {
                _updater.Dispose();
                _updater = null;
            }
#if !NET6_0_OR_GREATER
            if (_downloadClient != null)
            {
                _downloadClient.Dispose();
                _downloadClient = null;
            }
#endif

            GC.SuppressFinalize(this);
        }

        public int Invoke(string[] args)
        {


            return _rootCommand.Invoke(args);
        }
    }
}
