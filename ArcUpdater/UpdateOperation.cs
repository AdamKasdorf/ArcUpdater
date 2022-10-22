using ArcUpdater.CommandLine;

using System;
using System.IO;

namespace ArcUpdater
{
    /// <summary>
    /// Represents an operation to update or install ArcDPS assemblies.
    /// </summary>
    public class UpdateOperation : IFileSystemOperation
    {
        private readonly AssemblyVerifier _verifier;
        private readonly AssemblyUpdater _updater;
        private readonly bool _del;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateOperation"/> class using the specified <paramref name="verifier"/>, <paramref name="updater"/>, and deletion behavior.
        /// </summary>
        /// <param name="verifier">The verifier to use to verify assemblies.</param>
        /// <param name="updater">The updater to use to update assemblies.</param>
        /// <param name="del">Indicates whether the operation should overwrite files rather than send them to the Recycle Bin.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="verifier"/> or <paramref name="updater"/> is <see langword="null"/>.
        /// </exception>
        public UpdateOperation(AssemblyVerifier verifier, AssemblyUpdater updater, bool del)
        {
            if (verifier == null)
            {
                throw new ArgumentNullException(nameof(verifier));
            }

            if (updater == null)
            {
                throw new ArgumentNullException(nameof(updater));
            }

            _verifier = verifier;
            _updater = updater;
            _del = del;
        }

        public bool TargetDirectoryEmpty(FileSystemOperationState state)
        {
            return InstallToTargetDirectory(state);
        }

        public bool TargetDirectoryFound(FileSystemOperationState state)
        {
            return true;
        }

        public bool TargetDirectoryNotFound(FileSystemOperationState state)
        {
            DirectoryInfo directory = Directory.CreateDirectory(state.FullPath);
            
            if (directory.Exists)
            {
                return InstallToTargetDirectory(state);
            }

            return false;
        }

        public bool TargetFileFound(FileSystemOperationState state)
        {
            return UpdateFile(state);
        }

        public bool TargetFileNotFound(FileSystemOperationState state)
        {
            return InstallFile(state);
        }

        public bool FileFoundInTargetDirectory(FileSystemOperationState state)
        {
            return UpdateFile(state);
        }

        private bool InstallToTargetDirectory(FileSystemOperationState state)
        {
            string filePath = Path.Combine(state.FullPath, "d3d11.dll");
            FileSystemOperationState subState = new FileSystemOperationState(filePath);
            bool success = InstallFile(subState);
            
            if (subState.Cancel)
            {
                state.Cancel = true;
            }

            return success;
        }

        private bool InstallFile(FileSystemOperationState state)
        {
            // This method assumes the file does not already exists.

            if (!EnsureChecksumDownloaded())
            {
                state.Cancel = true;
                return false;
            }

            if (!EnsureLatestAssemblyRetrieved())
            {
                state.Cancel = true;
                return false;
            }

            if (WriteFile(state.FullPath, false))
            {
                Console.WriteLine("Assembly installed: " + state.FullPath);
                return true;
            }

            ConsoleHelper.WriteFileAccessError("Could not install assembly to file path: " + state.FullPath);
            return false;
        }

        private bool UpdateFile(FileSystemOperationState state)
        {
            // This method assumes the file already exists.

            if (!EnsureChecksumDownloaded())
            {
                state.Cancel = true;
                return false;
            }

            // Verifying currentness of file assembly.
            // If current, returns true. If not found or outdated, proceeds to write.
            // Returns false on thrown exceptions.
            string filePath = state.FullPath;
            bool couldVerify = false;
            bool isValid = false;

            try
            {
                using FileStream stream = File.OpenRead(filePath);
                using ArcAssembly assembly = new ArcAssembly(stream);
                couldVerify = _verifier.TryVerify(assembly, out isValid);
            }
            catch
            {
            }

            if (!couldVerify)
            {
                ConsoleHelper.WriteFileAccessError("Could not verify assembly at file path: " + filePath);
                return false;
            }

            if (isValid)
            {
                Console.WriteLine("Assembly is current at file path: " + filePath);
                return true;
            }

            if (!EnsureLatestAssemblyRetrieved())
            {
                state.Cancel = true;
                return false;
            }

            if (WriteFile(state.FullPath, !_del))
            {
                Console.WriteLine("Assembly updated: " + filePath);
                return true;
            }

            ConsoleHelper.WriteFileAccessError("Could not update asembly at file path: " + filePath);
            return false;
        }

        private bool WriteFile(string filePath, bool recycleOldFile)
        {
            if (recycleOldFile)
            {
                if (!FileHelper.TryRecycle(filePath))
                {
                    ConsoleHelper.WriteFileAccessError("Could not move file to Recycle Bin: " + filePath);
                    return false;
                }

                Console.WriteLine("Moved file to Recycle Bin: " + filePath);
            }

            return _updater.TryWrite(filePath);
        }

        private bool EnsureChecksumDownloaded()
        {
            if (_verifier.ChecksumDownloaded)
            {
                return true;
            }

            if (_verifier.TryDownloadChecksum())
            {
                return true;
            }

            ConsoleHelper.WriteErrorLine("Could not download the md5sum file.");
            return false;
        }

        private bool EnsureLatestAssemblyRetrieved()
        {
            // This method assumes _verifier has already downloaded the md55sum.

            if (_updater.AssemblyRetrieved)
            {
                return true;
            }

            if (_updater.TryLoadLocalAssemblyFile())
            {
                if (_verifier.TryVerify(_updater.Assembly, out bool isValid) && isValid)
                {
                    return true;
                }
            }

            if (_updater.TryDownloadAssembly())
            {
                if (_verifier.TryVerify(_updater.Assembly, out bool isValid))
                {
                    if (isValid)
                    {
                        return true;
                    }

                    ConsoleHelper.WriteErrorLine("The downloaded assembly file is corrupt.");
                }
                else
                {
                    ConsoleHelper.WriteErrorLine("Could not verify the integrity of the downloaded assembly file.");
                }
            }
            else
            {
                ConsoleHelper.WriteErrorLine("Could not download the assembly file.");
            }

            return false;
        }
    }
}
