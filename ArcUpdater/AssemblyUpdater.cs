using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcUpdater
{
    /// <summary>
    /// Provides methods for downloading, installing, and updating ArcDPS assemblies.
    /// </summary>
    public class AssemblyUpdater : IDisposable
    {
        private static bool AttemptedResolveLocalPath;
        private static string CachedLocalAssemblyFilePath;

        /// <summary>
        /// Gets the fully-qualified file path at which a copy of the current assembly should be locally stored. Returns <see langword="null"/> if could not be determined.
        /// </summary>
        public static string LocalAssemblyFilePath
        {
            get
            {
                if (!AttemptedResolveLocalPath)
                {
                    try 
                    { 
                        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        CachedLocalAssemblyFilePath = Path.Combine(localAppData, "ArcUpdater", "d3d11.dll");
                    }
                    catch
                    {
                    }

                    AttemptedResolveLocalPath = true;
                }

                return CachedLocalAssemblyFilePath;
            }
        }

        private readonly HttpClient _client;
        private ArcAssembly _assembly;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyUpdater"/> class using the specified download <paramref name="client"/>.
        /// </summary>
        /// <param name="client">The client used to download assemblies from the remote source.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="client"/> is <see langword="null"/>.</exception>
        public AssemblyUpdater(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            
            _client = client;
        }

        ~AssemblyUpdater()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets a value that indicates whether an assembly has been retrieved from either the local or remote source.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called on this <see cref="AssemblyUpdater"/>.</exception>
        public bool AssemblyRetrieved
        {
            get 
            { 
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(AssemblyUpdater));
                }

                return _assembly != null; 
            }
        }

        /// <summary>
        /// Gets the current assembly used to install or update files.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called on this <see cref="AssemblyUpdater"/>.</exception>
        public ArcAssembly Assembly
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(AssemblyUpdater));
                }

                return _assembly;
            }
        }

        /// <summary>
        /// Releases all resources used by the retrieved assembly.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // managed resources
            }

            DisposeAssembly();

            _disposed = true;
        }

        private void DisposeAssembly()
        {
            if (_assembly != null)
            {
                _assembly.Dispose();
                _assembly = null;
            }
        }

        /// <summary>
        /// Creates or overwrites a file at the specified path and writes to it the contents of the currently retrieved assembly. A return value indicates whether the file write operation succeeded.
        /// </summary>
        /// <param name="filePath">The path and name of the file to create.</param>
        /// <returns><see langword="true"/> if the file was written successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryCopyTo(string filePath)
        {
            if (_assembly != null)
            {
                try
                {
                    using (FileStream file = File.Create(filePath))
                    {
                        _assembly.CopyTo(file);
                        return true;
                    }
                }
                catch
                {
                }
            }
            
            return false;
        }

        /// <summary>
        /// Opens the local assembly file to be stored as the currently retrieved assembly. A return value indicates whether the file exists and was opened.
        /// </summary>
        /// <returns><see langword="true"/> if the file exists and was opened; otherwise, <see langword="false"/>.</returns>
        public bool TryLoadLocalAssemblyFile()
        {
            if (!_disposed)
            {
                DisposeAssembly();

                if (LocalAssemblyFilePath != null && File.Exists(LocalAssemblyFilePath))
                {
                    try
                    {
                        FileStream fileStream = File.Open(LocalAssemblyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        _assembly = new ArcAssembly(fileStream);
                        return true;
                    }
                    catch
                    {
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Downloads the most current assembly from the remote source to be stored as the currently retrieved assembly. A return value indicates whether the download succeeded.
        /// </summary>
        /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
        public bool TryDownloadAssembly()
        {
            if (_disposed)
            {
                return false;
            }

            DisposeAssembly();

            string path = LocalAssemblyFilePath;
            Stream assemblyStream = null;

            if (path != null)
            {
                try
                {
                    string directory = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directory);
                    assemblyStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (assemblyStream == null)
            {
                // Allows the download to be stored in temporary memory
                // even if local directory/file could not be created.
                assemblyStream = new MemoryStream();
            }

            try
            {
                using (Task download = DownloadAssemblyInternal(assemblyStream))
                {
                    if (download.Wait(10000) && download.IsCompletedSuccessfully)
                    {
                        _assembly = new ArcAssembly(assemblyStream);
                        return true;
                    }
                }
            }
            catch
            {
            }

            assemblyStream.Dispose();
            return false;
        }

        private async Task DownloadAssemblyInternal(Stream destination)
        {
            const string AssemblyUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";

            using (HttpResponseMessage response = await _client.GetAsync(AssemblyUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await response.Content.CopyToAsync(destination);
            }
        }
    }
}
