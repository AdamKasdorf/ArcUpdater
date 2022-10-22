using Microsoft.VisualBasic.FileIO;

using System;
using System.Collections.Generic;
using System.IO;

namespace ArcUpdater
{
    /// <summary>
    /// Contains helper methods for performing file system operations.
    /// </summary>
    public static class FileHelper
    {
        private static readonly string[] _validFileNames = { "d3d11.dll", "dxgi.dll", "d3d9.dll", "gw2addon_arcdps.dll" };

        /// <summary>
        /// Gets an array containing the file names (including extensions) that are appropriate for an ArcDPS assembly.
        /// </summary>
        public static string[] ValidFileNames
        {
            get
            {
                return _validFileNames;
            }
        }

        /// <summary>
        /// Determines whether <see cref="ValidFileNames"/> contains the specified file name.
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns><see langword="true"/> if <see cref="ValidFileNames"/> contains the file name; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidFileName(string fileName)
        {
            if (fileName != null)
            {
                for (int i = 0; i < _validFileNames.Length; i++)
                {
                    if (fileName.Equals(_validFileNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Enumerates the <see cref="ValidFileNames"/> combined with the specified <paramref name="directory"/> path.
        /// </summary>
        /// <param name="directory">The path with which the file names are combined.</param>
        /// <returns>The enumerated combined paths.</returns>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="directory"/> is <see langword="null"/>.
        /// </exception>
        public static IEnumerable<string> GetValidFilePaths(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            for (int i = 0; i < _validFileNames.Length; i++)
            {
                yield return Path.Combine(directory, _validFileNames[i]);
            }
        }

        /// <summary>
        /// Attempts to delete the specified file.
        /// </summary>
        /// <param name="filePath">The name of the file to be deleted. Wildcard characters are not supported.</param>
        /// <returns><see langword="true"/> if the file was deleted successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to send the specified file to the Recycle Bin.
        /// </summary>
        /// <param name="filePath">The name of the file to be sent to the Recycle Bin.</param>
        /// <returns><see langword="true"/> if the file was sent to the Recycle Bin successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryRecycle(string filePath)
        {
            try
            {
                FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
