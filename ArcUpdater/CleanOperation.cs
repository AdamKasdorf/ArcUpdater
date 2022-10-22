﻿using ArcUpdater.CommandLine;

using System;

namespace ArcUpdater
{
    /// <summary>
    /// Represents an operation to remove files.
    /// </summary>
    public class CleanOperation : IFileSystemOperation
    {
        private readonly bool _del;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanOperation"/> class with the specified deletion behavior.
        /// </summary>
        /// <param name="del">Indicates whether the operation should delete files rather than send them to the Recycle Bin.</param>
        public CleanOperation(bool del)
        {
            _del = del;
        }

        public bool TargetDirectoryEmpty(FileSystemOperationState state)
        {
            Console.WriteLine("No appropriately-named assemblies found in directory: " + state.FullPath);
            return true;
        }

        public bool TargetDirectoryFound(FileSystemOperationState state)
        {
            return true;
        }

        public bool TargetDirectoryNotFound(FileSystemOperationState state)
        {
            ConsoleHelper.WriteErrorLine("Could not find directory: " + state.FullPath);
            return true;
        }

        public bool TargetFileFound(FileSystemOperationState state)
        {
            return CleanFile(state.FullPath);
        }

        public bool TargetFileNotFound(FileSystemOperationState state)
        {
            return true;
        }

        public bool FileFoundInTargetDirectory(FileSystemOperationState state)
        {
            return CleanFile(state.FullPath);
        }

        private bool CleanFile(string filePath)
        {
            if (_del)
            {
                if (FileHelper.TryDelete(filePath))
                {
                    Console.WriteLine("Deleted file: " + filePath);
                    return true;
                }

                ConsoleHelper.WriteFileAccessError("Could not delete file: " + filePath);
            }
            else
            {
                if (FileHelper.TryRecycle(filePath))
                {
                    Console.WriteLine("Moved file to Recycle Bin: " + filePath);
                    return true;
                }
                
                ConsoleHelper.WriteFileAccessError("Could not move file to Recycle Bin: " + filePath);
            }

            return false;
        }
    }
}
