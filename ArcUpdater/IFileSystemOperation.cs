namespace ArcUpdater
{
    /// <summary>
    /// Provides methods that determine the behavior of an operation checking for the existance of directories and files.
    /// </summary>
    public interface IFileSystemOperation
    {
        /// <summary>
        /// Called when a target directory contains no files enumerated by a query.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool TargetDirectoryEmpty(FileSystemOperationState state);

        /// <summary>
        /// Called when a target directory is determined to exist.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool TargetDirectoryFound(FileSystemOperationState state);

        /// <summary>
        /// Called when a target directory is determined to not exist.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool TargetDirectoryNotFound(FileSystemOperationState state);

        /// <summary>
        /// Called when a target file is determined to exist.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool TargetFileFound(FileSystemOperationState state);

        /// <summary>
        /// Called when a target file is determined to not exist.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool TargetFileNotFound(FileSystemOperationState state);

        /// <summary>
        /// Called when a valid file is determined to exist in a target directory.
        /// </summary>
        /// <param name="state">The current state of the operation.</param>
        /// <returns><see langword="true"/> if the operation should continue; otherwise, <see langword="false"/>.</returns>
        bool FileFoundInTargetDirectory(FileSystemOperationState state);
    }
}
