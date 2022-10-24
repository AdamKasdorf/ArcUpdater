using System.Collections.Generic;

namespace ArcUpdater
{
    /// <summary>
    /// Represents the state of an operation being performed in a file system.
    /// </summary>
    public class FileSystemOperationState
    {
        private readonly string _fullPath;
        private readonly List<string> _repeats;
        private readonly CancellationState _cancelState;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemOperationState"/> class using the specified path.
        /// </summary>
        /// <param name="fullPath">The fully-qualified path that is the target of the operation.</param>
        public FileSystemOperationState(string fullPath, bool ignoreRepeats)
            : this(fullPath, ignoreRepeats ? new List<string>() : null , new CancellationState())
        {
        }

        protected FileSystemOperationState(string fullPath, List<string> repeats, CancellationState cancelState)
        {
            _fullPath = fullPath;
            _repeats = repeats;
            _cancelState = cancelState;
        }

        /// <summary>
        /// Creates a new state object using the specified path that shares the repeat behavior and cancellation state of those from which it is derived.
        /// </summary>
        /// <param name="fullPath">The fully-qualified path that is the target of the operation.</param>
        /// <returns>The derived state object.</returns>
        public FileSystemOperationState Derive(string fullPath)
        {
            return new FileSystemOperationState(fullPath, _repeats, _cancelState);
        }

        /// <summary>
        /// Gets the fully-qualified path that is the current target of the operation.
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
        public bool Cancel
        {
            get
            {
                return _cancelState.Cancelled;
            }

            set
            {
                _cancelState.Cancelled = value;
            }
        }

        /// <summary>
        /// Determines if the execution of the operation this <see cref="FileSystemOperationState"/> represents is configured to ignore repeat targets and files and whether <see cref="FullPath"/> has already been subject to operation.<br/>
        /// If so, it is added to the collection of paths to ignore in future checks.
        /// </summary>
        /// <returns><see langword="true"/> if the operation should ignore the path; otherwise, <see langword="false"/>.</returns>
        public bool CheckIgnoreAndAdd()
        {
            if (_repeats != null)
            {
                if (_repeats.Contains(_fullPath))
                {
                    return true;
                }

                _repeats.Add(_fullPath);
            }

            return false;
        }

        protected class CancellationState
        {
            public bool Cancelled;
        }
    }
}
