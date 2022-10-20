namespace ArcUpdater
{
    public class FileSystemOperationState
    {
        private readonly string _fullPath;

        public FileSystemOperationState(string fullPath)
        {
            _fullPath = fullPath;
        }

        public string FullPath
        {
            get
            {
                return _fullPath;
            }
        }

        public bool Cancel { get; set; }
    }
}
