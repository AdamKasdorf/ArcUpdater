using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace ArcUpdater.CommandLine
{
    internal class VerifyCommand : Command
    {
        private const string CommandName = "--verify";
        private const string CommandAlias = "-v";
        private const string CommandDescription = "Verify the currentness and integrity of appropriately-named assemblies in the current working directory or in the specified directories and file paths.";

        private readonly AssemblyVerifier _verifier;

        public VerifyCommand(AssemblyVerifier verifier)
            : base(CommandName, CommandDescription)
        {
            if (verifier == null)
            {
                throw new ArgumentNullException(nameof(verifier));
            }

            _verifier = verifier;

            AddAlias(CommandAlias);
            TreatUnmatchedTokensAsErrors = true;
            Handler = CommandHandler.Create<string[]>(Invoke);
        }

        private int Invoke(string[] paths)
        {
            if (!PathsArgument.TryResolve(paths, out IEnumerable<TargetPath> targets))
            {
                return ErrorCode.Failure;
            }

            VerifyOperation verify = new VerifyOperation(_verifier);
            TargetPathOperation operation = new TargetPathOperation(verify, FileHelper.GetValidFilePaths);
            bool success = operation.Execute(targets);
            return ErrorCode.GetValue(success);
        }
    }
}
