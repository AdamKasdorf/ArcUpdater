using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArcUpdater
{
    public class AssemblyVerifier
    {
        private readonly DownloadClient _downloadClient;
        private string _md5sum;

        public AssemblyVerifier(DownloadClient downloadClient)
        {
            if (downloadClient == null)
            {
                throw new ArgumentNullException(nameof(downloadClient));
            }

            _downloadClient = downloadClient;
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
                Task<byte[]> download = DownloadChecksum();

                if (download.Wait(10000) && download.IsCompletedSuccessfully)
                {
                    _md5sum = Encoding.UTF8.GetString(download.Result, 0, 32);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }
        
        private async Task<byte[]> DownloadChecksum()
        {
            using (HttpClient client = _downloadClient.Create())
            {
                const string MD5SumUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll.md5sum";

                using (HttpResponseMessage response = await client.GetAsync(MD5SumUrl, HttpCompletionOption.ResponseHeadersRead))
                { 
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }
    }
}
