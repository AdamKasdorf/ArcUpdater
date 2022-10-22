namespace ArcUpdater
{
    /// <summary>
    /// Represents the state of an operation being performed in a file system.
    /// </summary>
    public class FileSystemOperationState
    {
        private readonly string _fullPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemOperationState"/> class using the specified path.
        /// </summary>
        /// <param name="fullPath">The fully-qualified path that is the target of the operation.</param>
        public FileSystemOperationState(string fullPath)
        {
            _fullPath = fullPath;
        }

        /// <summary>
        /// Gets the fully-qualified path that is the target of the operation.
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
        }

        /// <summary>
        /// Gets or sets whether the operation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
