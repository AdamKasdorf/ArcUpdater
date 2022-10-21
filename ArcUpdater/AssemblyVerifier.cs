using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArcUpdater
{
    public class AssemblyVerifier
    {
        private readonly HttpClient _client;
        private string _md5sum;

        public AssemblyVerifier(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _client = client;
        }

        public bool ChecksumDownloaded
        {
            get 
            { 
                return _md5sum != null; 
            }
        }

        public bool TryVerify(ArcAssembly assembly, out bool result)
        {
            if (ChecksumDownloaded)
            {
                try
                {
                    string fileSum = assembly.ComputeChecksum();

                    if (fileSum.Length != _md5sum.Length)
                    {
                        result = false;
                        return true;
                    }

                    for (int i = 0; i < _md5sum.Length; i++)
                    {
                        if (fileSum[i] != _md5sum[i])
                        {
                            result = false;
                            return true;
                        }
                    }

                    result = true;
                    return true;
                }
                catch
                {
                }
            }

            result = false;
            return false;
        }

        public bool TryDownloadChecksum()
        {
            try
            {
                using (Task<byte[]> download = DownloadChecksum())
                {
                    if (download.Wait(10000) && download.IsCompletedSuccessfully)
                    {
                        _md5sum = Encoding.UTF8.GetString(download.Result, 0, 32);
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }
        
        private async Task<byte[]> DownloadChecksum()
        {
            const string MD5SumUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll.md5sum";

            using (HttpResponseMessage response = await _client.GetAsync(MD5SumUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
    }
}
