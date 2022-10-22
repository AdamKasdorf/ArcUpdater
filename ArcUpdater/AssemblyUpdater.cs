using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcUpdater
{
    public class AssemblyUpdater : IDisposable
    {
        private static string CachedLocalAssemblyFilePath;

        private static string LocalAssemblyFilePath
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
        /// 
        /// </summary>
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

        public bool TryWrite(string filePath)
        {
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
                    FileStream fileStream = File.Open(LocalAssemblyFilePath, FileMode.Open, FileAccess.ReadWrite);
                    _assembly = new ArcAssembly(fileStream);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

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
