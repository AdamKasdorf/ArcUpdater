using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace ArcUpdater.CommandLine
{
    internal class UpdateCommand : Command
    {
        private const string CommandName = "--update";
        private const string CommandAlias = "-u";
        private const string CommandDesc = "Check for updates to ArcDPS, download the latest version, verify the integrity of the assembly, and install to the current working directory or specified directories and file paths."
            + " If a directory is specified and does not contain any existing appropriately-named assembly, the assembly will be installed using the default file name.";

        private readonly AssemblyVerifier _verifier;
        private readonly AssemblyUpdater _updater;

        public UpdateCommand(AssemblyVerifier verifier, AssemblyUpdater updater)
            : base(CommandName, CommandDesc)
        {
            if (verifier == null)
            {
                throw new ArgumentNullException(nameof(verifier));
            }

            if (updater == null)
            {
                throw new ArgumentNullException(nameof(updater));
            }

            _verifier = verifier;
            _updater = updater;

            AddAlias(CommandAlias);
            AddOption(new DeleteOption());
            IsHidden = true;
            Handler = CommandHandler.Create<string[], bool>(Invoke);
        }

        private int Invoke(string[] paths, bool del)
        {
            if (!PathsArgument.TryResolve(paths, out IEnumerable<TargetPath> targets))
            {
                return ErrorCode.Failure;
            }

            UpdateOperation update = new UpdateOperation(_verifier, _updater, del);
            TargetPathOperation operation = new TargetPathOperation(update, FileHelper.GetValidFilePaths);
            bool success = operation.Execute(targets);
            return ErrorCode.GetValue(success);
        }
    }
}
