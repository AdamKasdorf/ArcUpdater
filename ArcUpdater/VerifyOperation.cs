using ArcUpdater.CommandLine;

using System;
using System.IO;

namespace ArcUpdater
{
    public class VerifyOperation : IFileSystemOperation
    {
        private readonly AssemblyVerifier _verifier;
        
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
            ConsoleHelper.WriteErrorLine("Could not find file at path: " + state.FullPath);
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
                        Console.WriteLine("Assembly is current at file path: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine("Assembly is outdated or corrupt at file path: " + filePath);
                    }

                    return true;
                }
            }
            catch
            {
            }

            // TryVerify failed (returned false) or an exception was thrown.
            ConsoleHelper.WriteFileAccessError("Could not verify assembly at file path: " + filePath);
            return false;
        }
    }
}
