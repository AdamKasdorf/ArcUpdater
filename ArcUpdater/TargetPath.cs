using System;
using System.IO;

namespace ArcUpdater
{
    /// <summary>
    /// Represents a fully-qualified path.
    /// </summary>
    public class TargetPath
    {
        /// <summary>
        /// Gets a <see cref="TargetPath"/> that represents the current working directory.
        /// </summary>
        public static TargetPath CurrentDirectory
        {
            get
            {
                string cd = Environment.CurrentDirectory;
                if (!Path.EndsInDirectorySeparator(cd))
                {
                    cd += Path.DirectorySeparatorChar;
                }
                return new TargetPath(cd, true);
            }
        }

        private string _fullPath;
        private bool _isDirectory;

        private TargetPath(string fullPath, bool isDirectory)
        {
            _fullPath = fullPath;
            _isDirectory = isDirectory;
        }

        /// <summary>
        /// Gets the fully-qualified path that this <see cref="TargetPath"/> represents.
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the path that this <see cref="TargetPath"/> represents should be treated as a directory.
        /// </summary>
        public bool IsDirectory
        {
            get
            {
                return _isDirectory;
            }
        }

        /// <summary>
        /// Resolves the fully-qualified path from the specified <paramref name="path"/>.<br/>
        /// Expands environment variables, appends '.dll' to valid file names that lack extensions when appropriate, and accepts paths relative to the current working directory.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>A <see cref="TargetPath"/> that represents the fully-qualified path.</returns>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="path"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="PathResolutionException">
        /// The specified <paramref name="path"/> contains a character that is not allowed in path names 
        /// or contains a file name that is not appropriate for an ArcDPS assembly.
        /// </exception>
        public static TargetPath Resolve(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            bool isDirectory;
            string fullPath = Environment.ExpandEnvironmentVariables(path);

            try
            {
                fullPath = Path.GetFullPath(fullPath, Environment.CurrentDirectory);
            }
            catch (ArgumentException)
            {
                // At this point, fullPath cannot be null and Environment.CurrentDirectory is fully-qualified.
                // GetFullPath should only throw ArgumentException if fullPath
                // contains an invalid character defined in GetInvalidPathChars().
                throw new PathResolutionException(fullPath, "Invalid character in path: " + fullPath);
            }

            string fileName = Path.GetFileName(fullPath);

            if (fileName == string.Empty || Directory.Exists(fullPath))
            {
                isDirectory = true;

                if (!Path.EndsInDirectorySeparator(fullPath))
                {
                    fullPath += Path.DirectorySeparatorChar;
                }
            }
            else
            {
                isDirectory = false;
                string extension = Path.GetExtension(fullPath);

                if (extension == string.Empty)
                {
                    fileName += ".dll";
                    fullPath += ".dll";
                }

                if (!FileHelper.IsValidFileName(fileName))
                {
                    throw new PathResolutionException(fullPath, "Invalid file name for assembly in path: " + fullPath + Environment.NewLine
                        + "Valid names are 'd3d11.dll', 'dxgi.dll', 'd3d9.dll' (deprecated), and 'gw2addon_arcdps.dll'.");
                }
            }

            return new TargetPath(fullPath, isDirectory);
        }
    }
}
