using Microsoft.VisualBasic.FileIO;

using System;
using System.Collections.Generic;
using System.IO;

namespace ArcUpdater
{
    public static class FileHelper
    {
        private static readonly string[] _validFileNames = { "d3d11.dll", "dxgi.dll", "d3d9.dll", "gw2addon_arcdps.dll" };

        public static string[] ValidFileNames
        {
            get
            {
                return _validFileNames;
            }
        }

        public static bool IsValidFileName(string fileName)
        {
            for (int i = 0; i < _validFileNames.Length; i++)
            {
                if (fileName.Equals(_validFileNames[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

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
