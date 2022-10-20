using ArcUpdater.CommandLine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArcUpdater
{
    public delegate IEnumerable<string> DirectoryQuery(string path);

    public class TargetPathOperation
    {
        private readonly IFileSystemOperation _operation;
        private readonly DirectoryQuery _targetDirectoryFileQuery;

        public TargetPathOperation(IFileSystemOperation operation)
            : this(operation, null)
        {
        }

        public TargetPathOperation(IFileSystemOperation operation, DirectoryQuery targetDirectoryFileQuery)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }
            
            _operation = operation;
            _targetDirectoryFileQuery = targetDirectoryFileQuery ?? Directory.EnumerateFiles;
        }

        public bool Execute(TargetPath target)
        {
            FileSystemOperationState state = new FileSystemOperationState(target.FullPath);

            if (target.IsDirectory)
            {
                return TargetDirectory(state);
            }
            return TargetFile(state);
        }

        public bool Execute(IEnumerable<TargetPath> targets)
        {
            bool success = true;

            foreach (TargetPath target in targets)
            {
                FileSystemOperationState state = new FileSystemOperationState(target.FullPath);

                if (target.IsDirectory)
                {
                    if (!TargetDirectory(state))
                    {
                        success = false;
                    }
                }
                else
                {
                    if (!TargetFile(state))
                    {
                        success = false;
                    }
                }

                if (state.Cancel)
                {
                    return false;
                }
            }

            return success;
        }

        private bool TargetFile(FileSystemOperationState state)
        {
            if (File.Exists(state.FullPath))
            {
                return _operation.TargetFileFound(state);
            }

            return _operation.TargetFileNotFound(state);
        }

        private bool TargetDirectory(FileSystemOperationState state)
        {
            string path = state.FullPath;

            if (Directory.Exists(path))
            {
                if (_operation.TargetDirectoryFound(state))
                {
                    QueryTargetDirectory(state);
                }

                return false;
            }

            return _operation.TargetDirectoryNotFound(state);
        }

        private bool QueryTargetDirectory(FileSystemOperationState state)
        {
            string[] filePaths;

            try
            {
                filePaths = _targetDirectoryFileQuery(state.FullPath).ToArray();
            }
            catch
            {
                ConsoleHelper.WriteErrorLine("Could not query files in directory: " + state.FullPath);
                return false;
            }

            int filesFound = 0;
            bool success = true;

            if (filePaths != null)
            {
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath))
                    {
                        filesFound++;
                        FileSystemOperationState subState = new FileSystemOperationState(filePath);

                        if (!_operation.FileFoundInTargetDirectory(subState))
                        {
                            if (subState.Cancel)
                            {
                                state.Cancel = true;
                                return false;
                            }

                            success = false;
                        }
                    }
                }
            }

            if (filesFound == 0)
            {
                return _operation.TargetDirectoryEmpty(state);
            }

            return success;
        }
    }
}
