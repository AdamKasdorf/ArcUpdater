using ArcUpdater.CommandLine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArcUpdater
{
    /// <summary>
    /// Represents a method used to query directories.
    /// </summary>
    /// <param name="path">The path of the directory to query.</param>
    public delegate IEnumerable<string> DirectoryQuery(string path);

    /// <summary>
    /// Represents an operation performed on target paths and files found in queried target directories.
    /// </summary>
    public class TargetPathOperation
    {
        private readonly IFileSystemOperation _operation;
        private readonly DirectoryQuery _targetDirectoryFileQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetPathOperation"/> class using the specified <paramref name="operation"/> and <see cref="Directory.EnumerateFiles"/> to query found directories.
        /// </summary>
        /// <param name="operation">The operation to perform.</param>
        public TargetPathOperation(IFileSystemOperation operation)
            : this(operation, Directory.EnumerateFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetPathOperation"/> class using the specified <paramref name="operation"/> and query delegate.
        /// </summary>
        /// <param name="operation">The operation to perform.</param>
        /// <param name="targetDirectoryFileQuery">The delegate used to query found directories.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="operation"/> or <paramref name="targetDirectoryFileQuery"/> is <see langword="null"/>.
        /// </exception>
        public TargetPathOperation(IFileSystemOperation operation, DirectoryQuery targetDirectoryFileQuery)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (targetDirectoryFileQuery == null)
            {
                throw new ArgumentException(nameof(targetDirectoryFileQuery));
            }
            
            _operation = operation;
            _targetDirectoryFileQuery = targetDirectoryFileQuery;
        }

        /// <summary>
        /// Executes the operation on the specified <paramref name="target"/> using the specified behavior for repeats.
        /// </summary>
        /// <param name="target">The target of the operation.</param>
        /// <param name="ignoreRepeats">Specifies that the operation should ignore repeat targets and queried files.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool Execute(TargetPath target, bool ignoreRepeats = true)
        {
            FileSystemOperationState state = new FileSystemOperationState(target.FullPath, ignoreRepeats);

            if (target.IsDirectory)
            {
                return TargetDirectory(state);
            }

            return TargetFile(state);
        }

        /// <summary>
        /// Executes the operation on the specified <paramref name="targets"/> using the specified behavior for repeats.
        /// </summary>
        /// <param name="target">The targets of the operation.</param>
        /// /// <param name="ignoreRepeats">Specifies that the operation should ignore repeat targets and queried files.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool Execute(IEnumerable<TargetPath> targets, bool ignoreRepeats = true)
        {
            FileSystemOperationState state = new FileSystemOperationState(null, ignoreRepeats);
            bool success = true;

            foreach (TargetPath target in targets)
            {
                FileSystemOperationState substate = state.Derive(target.FullPath);

                if (substate.CheckIgnoreAndAdd())
                {
                    continue;
                }

                if (target.IsDirectory)
                {
                    if (!TargetDirectory(substate))
                    {
                        success = false;
                    }
                }
                else
                {
                    if (!TargetFile(substate))
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
                    return QueryTargetDirectory(state);
                }

                return false;
            }

            return _operation.TargetDirectoryNotFound(state);
        }

        private bool QueryTargetDirectory(FileSystemOperationState state)
        {
            List<string> filePaths;

            try
            {
                // Forcing evaluation
                filePaths = _targetDirectoryFileQuery(state.FullPath).ToList();
            }
            catch
            {
                ConsoleHelper.WriteErrorLine("Could not perform query on directory: " + state.FullPath);
                return false;
            }

            int filesFound = 0;
            bool success = true;

            foreach (string filePath in filePaths)
            {
                FileSystemOperationState substate = state.Derive(filePath);
                bool shouldIgnore = substate.CheckIgnoreAndAdd();

                if (File.Exists(filePath))
                {
                    filesFound++;

                    if (shouldIgnore)
                    {
                        // Wait to continue until AFTER filesFound incremented
                        // for expected TargetDirectoryEmpty behavior.
                        continue;
                    }

                    if (!_operation.FileFoundInTargetDirectory(substate))
                    {
                        if (substate.Cancel)
                        {
                            return false;
                        }

                        success = false;
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
