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
            : this(operation, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetPathOperation"/> class using the specified <paramref name="operation"/> and query delegate.
        /// </summary>
        /// <param name="operation">The operation to perform.</param>
        /// <param name="targetDirectoryFileQuery">The delegate used to query found directories.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="targetDirectoryFileQuery"/> is <see langword="null"/>.
        /// </exception>
        public TargetPathOperation(IFileSystemOperation operation, DirectoryQuery targetDirectoryFileQuery)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }
            
            _operation = operation;
            _targetDirectoryFileQuery = targetDirectoryFileQuery ?? Directory.EnumerateFiles;
        }

        /// <summary>
        /// Executes the operation on the specified <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target of the operation.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
        public bool Execute(TargetPath target)
        {
            FileSystemOperationState state = new FileSystemOperationState(target.FullPath);

            if (target.IsDirectory)
            {
                return TargetDirectory(state);
            }
            return TargetFile(state);
        }

        /// <summary>
        /// Executes the operation on the specified <paramref name="targets"/>.
        /// </summary>
        /// <param name="target">The targets of the operation.</param>
        /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
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
                ConsoleHelper.WriteErrorLine("Could not perform directory query.");
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
                        FileSystemOperationState substate = new FileSystemOperationState(filePath);

                        if (!_operation.FileFoundInTargetDirectory(substate))
                        {
                            if (substate.Cancel)
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
