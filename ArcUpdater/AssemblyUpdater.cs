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

        private readonly DownloadClient _downloadClient;
        private ArcAssembly _assembly;
        private bool _disposed;

        public AssemblyUpdater(DownloadClient downloadClient)
        {
            if (downloadClient == null)
            {
                throw new ArgumentNullException(nameof(downloadClient));
            }

            _downloadClient = downloadClient;
        }

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
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DisposeAssembly();
            GC.SuppressFinalize(this);
        }

        public bool TryLoadAssemblyFile()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AssemblyUpdater));
            }

            DisposeAssembly();

            if (File.Exists(LocalAssemblyFilePath))
            {
                try
                {
                    FileStream fileStream = File.Open(LocalAssemblyFilePath, FileMode.Open, FileAccess.ReadWrite);
                    _assembly = new ArcAssembly(fileStream);
                    return true;
                }
                catch
                {
                }
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

            try
            {
                Task download = DownloadAssembly(LocalAssemblyFilePath);

                if (download.Wait(10000) && download.IsCompletedSuccessfully)
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

        private async Task DownloadAssembly(string destFilePath)
        {
            using (HttpClient client = _downloadClient.Create())
            {
                const string AssemblyUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll";

                using (HttpResponseMessage response = await client.GetAsync(AssemblyUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        string directory = Path.GetDirectoryName(destFilePath);
                        Directory.CreateDirectory(directory);

                        using (FileStream file = File.Open(destFilePath, FileMode.Create))
                        {
                            await responseStream.CopyToAsync(file);
                        }
                    }
                }
            }
        }

        public bool TryWrite(string filePath)
        {
            if (AssemblyRetrieved)
            {
                try
                {
                    _assembly.WriteToFile(filePath);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        private void DisposeAssembly()
        {
            if (_assembly != null)
            {
                _assembly.Dispose();
                _assembly = null;
            }
        }
    }
}
