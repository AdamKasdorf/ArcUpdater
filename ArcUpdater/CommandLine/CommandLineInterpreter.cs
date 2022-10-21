using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net.Http;

namespace ArcUpdater.CommandLine
{
    internal class CommandLineInterpreter : IDisposable
    {
        private const string CommandDesc = "Check for updates to ArcDPS, download the latest version, verify the integrity of the assembly, and install to the current working directory or specified directories and file paths.";

        private readonly RootCommand _rootCommand;
        private HttpClient _client;
        private AssemblyVerifier _verifier;
        private AssemblyUpdater _updater;

        private bool _disposed;

        public CommandLineInterpreter()
        {
            _client = new HttpClient();
            _verifier = new AssemblyVerifier(_client);
            _updater = new AssemblyUpdater(_client);

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
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // managed resources
            }

            if (_updater != null)
            {
                _updater.Dispose();
                _updater = null;
            }

            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            _disposed = true;
        }

        public int Invoke(string[] args)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CommandLineInterpreter));
            }

            return _rootCommand.Invoke(args);
        }
    }
}
