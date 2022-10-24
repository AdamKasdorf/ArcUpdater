using ArcUpdater.CommandLine;

using System;
using System.IO;

namespace ArcUpdater
{
    /// <summary>
    /// Represents an operation to verify the currentness and integrity of ArcDPS assemblies.
    /// </summary>
    public class VerifyOperation : IFileSystemOperation
    {
        private readonly AssemblyVerifier _verifier;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VerifyOperation"/> class using the specified <paramref name="verifier"/>.
        /// </summary>
        /// <param name="verifier">The verifier to use to verify found assemblies.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="verifier"/> is <see langword="null"/>.
        /// </exception>
        public VerifyOperation(AssemblyVerifier verifier)
        {
            if (verifier == null)
            {
                throw new ArgumentNullException(nameof(verifier));
            }

            _verifier = verifier;
        }

        public bool TargetDirectoryEmpty(FileSystemOperationState state)
        {
            Console.WriteLine("No appropriately-named assemblies found in directory: " + state.FullPath);
            return true;
        }

        public bool TargetDirectoryFound(FileSystemOperationState state)
        {
            return true;
        }

        public bool TargetDirectoryNotFound(FileSystemOperationState state)
        {
            ConsoleHelper.WriteErrorLine("Could not find directory: " + state.FullPath);
            return false;
        }

        public bool TargetFileFound(FileSystemOperationState state)
        {
            return TryVerifyFile(state);
        }

        public bool TargetFileNotFound(FileSystemOperationState state)
        {
            ConsoleHelper.WriteErrorLine("Could not find file: " + state.FullPath);
            return false;
        }

        public bool FileFoundInTargetDirectory(FileSystemOperationState state)
        {
            return TryVerifyFile(state);
        }

        private bool TryVerifyFile(FileSystemOperationState state)
        {
            if (!_verifier.ChecksumDownloaded && !_verifier.TryDownloadChecksum())
            {
                ConsoleHelper.WriteErrorLine("Could not download the md5sum file.");
                state.Cancel = true;
                return false;
            }

            string filePath = state.FullPath;

            try
            {
                using FileStream stream = File.OpenRead(filePath);
                using ArcAssembly assembly = new ArcAssembly(stream);

                if (_verifier.TryVerify(assembly, out bool isValid))
                {
                    if (isValid)
                    {
                        Console.WriteLine("Assembly is current: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine("Assembly is outdated or corrupt: " + filePath);
                    }

                    return true;
                }
            }
            catch
            {
            }

            // TryVerify failed (returned false) or an exception was thrown.
            ConsoleHelper.WriteFileAccessError("Could not verify assembly: " + filePath);
            return false;
        }
    }
}
