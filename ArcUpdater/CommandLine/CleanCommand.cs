using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace ArcUpdater.CommandLine
{
    internal class CleanCommand : Command
    {
        private const string CommandName = "--clean";
        private const string CommandAlias = "-c";
        private const string CommandDescription = "Remove all appropriately-named assemblies from the current working directory or from the specified directories and file paths.";

        public CleanCommand()
            : base(CommandName, CommandDescription)
        {
            AddAlias(CommandAlias);
            AddOption(new DeleteOption());

            TreatUnmatchedTokensAsErrors = true;
            Handler = CommandHandler.Create<string[], bool>(Invoke);
        }

        private static int Invoke(string[] paths, bool del)
        {
            if (!PathsArgument.TryResolve(paths, out IEnumerable<TargetPath> targets))
            {
                return ErrorCode.Failure;
            }

            TargetPathOperation operation = new TargetPathOperation(new CleanOperation(del), FileHelper.GetValidFilePaths);
            bool success = operation.Execute(targets);
            return ErrorCode.GetValue(success);
        }
    }
}
