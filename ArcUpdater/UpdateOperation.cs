using ArcUpdater.CommandLine;

using System;
using System.IO;
using System.Runtime.InteropServices;

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
            try
            {
                Directory.CreateDirectory(state.FullPath);
            }
            catch
            {
                ConsoleHelper.WriteErrorLine("Could not create directories or subdirectories in path: " + state.FullPath);
                ConsoleHelper.WriteErrorLine(SR.LocationWrite);
                return false;
            }

            return InstallToTargetDirectory(state);
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
            return InstallFile( state.Derive(filePath) );
        }

        private bool InstallFile(FileSystemOperationState state)
        {
            // This method assumes the file does not already exists.

            if ( !(EnsureChecksumDownloaded() && EnsureLatestAssemblyRetrieved()) )
            {
                state.Cancel = true;
                return false;
            }

            if (WriteFile(state.FullPath, false))
            {
                Console.WriteLine("Assembly installed: " + state.FullPath);
                return true;
            }

            ConsoleHelper.WriteErrorLine("Could not install assembly: " + state.FullPath);
            ConsoleHelper.WriteError(SR.LocationWrite);
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

            // Verifying currentness and integrity of file.
            // If current and not current, returns true. Otherwise, proceeds to write.
            // Returns false on thrown exceptions.

            string filePath = state.FullPath;
            bool couldVerify = false;
            bool isValid = false;

            try
            {
                FileStream stream = File.OpenRead(filePath);

                using (ArcAssembly assembly = new ArcAssembly(stream))
                {
                    couldVerify = _verifier.TryVerify(assembly, out isValid);
                }
            }
            catch
            {
            }

            if (!couldVerify)
            {
                ConsoleHelper.WriteErrorLine("Could not verify assembly: " + filePath);
                ConsoleHelper.WriteErrorLine(SR.ExistingFileAccess);
                return false;
            }

            if (isValid)
            {
                Console.WriteLine("Assembly is current: " + filePath);
                return true;
            }

            // Assembly is determined to be outdated or corrupt.
            // Ensures latest assembly is retrieved and updates file.

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

            ConsoleHelper.WriteErrorLine("Could not update asembly: " + filePath);
            ConsoleHelper.WriteErrorLine(SR.ExistingFileWrite);
            return false;
        } 

        private bool WriteFile(string filePath, bool recycleOldFile)
        {
            // No need for messages here, since this will necessarily be followed
            // messages indicating success or failure of the install/update.

            if (recycleOldFile && !FileHelper.TryRecycle(filePath))
            {
                return false;
            }

            return _updater.TryCopyTo(filePath);
        }

        private bool EnsureChecksumDownloaded()
        {
            if (_verifier.ChecksumDownloaded || _verifier.TryDownloadChecksum())
            {
                return true;
            }

            ConsoleHelper.WriteErrorLine("Could not download the md5sum file.");
            return false;
        }

        private bool EnsureLatestAssemblyRetrieved()
        {
            // This method assumes _verifier has already downloaded the md55sum.

            bool isValid;

            if (_updater.AssemblyRetrieved)
            {
                return true;
            }

            if (_updater.TryLoadLocalAssemblyFile())
            {
                if (_verifier.TryVerify(_updater.Assembly, out isValid) && isValid)
                {
                    return true;
                }
            }

            if (!_updater.TryDownloadAssembly())
            {
                ConsoleHelper.WriteErrorLine("Could not download the assembly.");
                return false;
            }

            if (!_verifier.TryVerify(_updater.Assembly, out isValid))
            {
                ConsoleHelper.WriteErrorLine("Could not verify the integrity of the downloaded assembly.");
                return false;
            }

            if (isValid)
            {
                return true;
            }

            ConsoleHelper.WriteErrorLine("The downloaded assembly is corrupt.");
            return false;
        }
    }
}
