using System;
using System.IO;

namespace ArcUpdater
{
    public class TargetPath
    {
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

        public string FullPath
        {
            get
            {
                return _fullPath;
            }
        }

        public bool IsDirectory
        {
            get
            {
                return _isDirectory;
            }
        }

        public static TargetPath Resolve(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string fullPath = Environment.ExpandEnvironmentVariables(path);
            bool isDirectory;

            if (TryFindFirstChar(fullPath, Path.GetInvalidPathChars(), out char invalidChar))
            {
                throw new PathResolutionException(fullPath, "Invalid character '" + invalidChar + "' in path: " + fullPath);
            }

            fullPath = Path.GetFullPath(fullPath, Environment.CurrentDirectory);
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
                        + "Valid names are 'd3d11.dll', 'dxgi.dll', and 'd3d9.dll' (deprecated).");
                }
            }

            return new TargetPath(fullPath, isDirectory);
        }

        private static bool TryFindFirstChar(string value, char[] chars, out char first)
        {
            for (int i = 0; i < value.Length; i++)
            {
                for (int c = 0; c < chars.Length; c++)
                {
                    if (value[i] == chars[c])
                    {
                        first = chars[c];
                        return true;
                    }
                }
            }

            first = (char)0;
            return false;
        }
    }
}
