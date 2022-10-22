using System;

namespace ArcUpdater
{
    /// <summary>
    /// Represents errors that occur while attempting to resolve a path.
    /// </summary>
    public class PathResolutionException : Exception
    {
        private string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathResolutionException"/> class using the specified <paramref name="path"/> and error <paramref name="message"/>.
        /// </summary>
        /// <param name="path">The path being resolved when the error occured.</param>
        /// <param name="message">The message that describes the error.</param>
        public PathResolutionException(string path, string message)
            : base(message)
        {
            _path = path;
        }

        /// <summary>
        /// Gets the path being resolved when the error occured.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }
    }
}
