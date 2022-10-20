using System.Collections.Generic;
using System.CommandLine;

namespace ArcUpdater.CommandLine
{
    internal class PathsArgument : Argument<string[]>
    {
        public PathsArgument()
        {
            Name = "paths";
            Description = "The target directories or file paths. If not provided, operations are performed in the current working directory.";
            Arity = ArgumentArity.ZeroOrMore;
            IsHidden = false;
        }

        public static bool TryResolve(string[] paths, out IEnumerable<TargetPath> targets)
        {
            if (paths.Length == 0)
            {
                return TryGetCurrentDirectory(out targets);
            }

            List<TargetPath> resolvedPaths = new List<TargetPath>(paths.Length);
            bool exceptionThrown = false;

            for (int i = 0; i < paths.Length; i++)
            {
                try
                {
                    TargetPath path = TargetPath.Resolve(paths[i]);
                    AddIfUnique(resolvedPaths, path);
                }
                catch (PathResolutionException e)
                {
                    ConsoleHelper.WriteErrorLine(e.Message);
                    exceptionThrown = true;
                }
                catch
                {
                    ConsoleHelper.WriteErrorLine("Could not resolve specified path: " + paths[i]);
                    exceptionThrown = true;
                }
            }

            if (exceptionThrown)
            {
                targets = null;
                return false;
            }

            targets = resolvedPaths;
            return true;
        }

        private static bool TryGetCurrentDirectory(out IEnumerable<TargetPath> targets)
        {
            try
            {
                targets = new TargetPath[] { TargetPath.CurrentDirectory };
                return true;
            }
            catch
            {
                ConsoleHelper.WriteErrorLine("Could not determine current working directory.");
            }

            targets = null;
            return false;
        }

        private static void AddIfUnique(List<TargetPath> paths, TargetPath path)
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (path.FullPath == paths[i].FullPath)
                {
                    return;
                }
            }

            paths.Add(path);
        }
    }
}
