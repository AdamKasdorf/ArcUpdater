namespace ArcUpdater
{
    public interface IFileSystemOperation
    {
        public bool TargetDirectoryEmpty(FileSystemOperationState state);

        public bool TargetDirectoryFound(FileSystemOperationState state);

        public bool TargetDirectoryNotFound(FileSystemOperationState state);

        public bool TargetFileFound(FileSystemOperationState state);

        public bool TargetFileNotFound(FileSystemOperationState state);

        public bool FileFoundInTargetDirectory(FileSystemOperationState state);
    }
}
