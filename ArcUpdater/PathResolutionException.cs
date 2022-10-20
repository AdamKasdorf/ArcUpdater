using System;

namespace ArcUpdater
{
    public class PathResolutionException : Exception
    {
        private string _fullPath;

        public PathResolutionException(string fullPath, string message)
            : base(message)
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
    }
}
