using ArcUpdater.CommandLine;

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
            return true;
        }

        public bool TargetDirectoryFound(FileSystemOperationState state)
        {
            return true;
        }

        public bool TargetDirectoryNotFound(FileSystemOperationState state)
        {
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

                ConsoleHelper.WriteErrorLine("Could not delete file: " + filePath);
                ConsoleHelper.WriteErrorLine(SR.ExistingFileWrite);
            }
            else
            {
                if (FileHelper.TryRecycle(filePath))
                {
                    Console.WriteLine("Moved file to Recycle Bin: " + filePath);
                    return true;
                }
                
                ConsoleHelper.WriteErrorLine("Could not move file to Recycle Bin: " + filePath);
                ConsoleHelper.WriteErrorLine(SR.ExistingFileWrite);
            }

            return false;
        }
    }
}
