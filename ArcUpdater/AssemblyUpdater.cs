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
        private static string CachedLocalAssemblyFilePath;

        /// <summary>
        /// Gets the fully-qualified file path at which a copy of the current assembly should be locally stored.
        /// </summary>
        public static string LocalAssemblyFilePath
        {
            get
            {
                if (CachedLocalAssemblyFilePath == null)
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    CachedLocalAssemblyFilePath = Path.Combine(localAppData, "ArcUpdater", "d3d11.dll");
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
        /// Attempts to create or overwrite a file at the specified path and write the contents of the current assembly.
        /// </summary>
        /// <param name="filePath">The path and name of the file to create.</param>
        /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called on this <see cref="AssemblyUpdater"/>.</exception>
        public bool TryCopyTo(string filePath)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssemblyUpdater));
            }

            if (AssemblyRetrieved)
            {
                try
                {
                    using (FileStream file = File.Create(filePath))
                    {
                        _assembly.CopyTo(file);
                    }

                    return true;
                }
                catch
                {
                }
            }
            
            return false;
        }

        /// <summary>
        /// Attempts to open the locally-stored assembly file.
        /// </summary>
        /// <returns><see langword="true"/> if the file exists and was opened; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called on this <see cref="AssemblyUpdater"/>.</exception>
        public bool TryLoadLocalAssemblyFile()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssemblyUpdater));
            }

            DisposeAssembly();

            try
            {
                if (File.Exists(LocalAssemblyFilePath))
                {
                    FileStream fileStream = File.Open(LocalAssemblyFilePath, FileMode.Open, FileAccess.Read);
                    _assembly = new ArcAssembly(fileStream);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Attempts to download the most current assembly from the remote source.
        /// </summary>
        /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called on this <see cref="AssemblyUpdater"/>.</exception>
        public bool TryDownloadAssembly()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssemblyUpdater));
            }

            DisposeAssembly();

            FileStream assemblyStream = null;
            
            try
            {
                // Creating new file with persistent backing stream that can be reused
                // to verify integrity and then copy to a new location.
                string path = LocalAssemblyFilePath;
                string directory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(directory);
                assemblyStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite);

                using (Task download = DownloadAssembly(assemblyStream))
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

            assemblyStream?.Dispose();
            return false;
        }

        private async Task DownloadAssembly(Stream assemblyStream)
        {
            const string AssemblyUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";

            using (HttpResponseMessage response = await _client.GetAsync(AssemblyUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await response.Content.CopyToAsync(assemblyStream);
            }
        }
    }
}
