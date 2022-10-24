using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArcUpdater
{
    /// <summary>
    /// Provides methods to verify the currentness and integrity of ArcDPS assemblies.
    /// </summary>
    public class AssemblyVerifier
    {
        private readonly HttpClient _client;
        private string _md5sum;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyVerifier"/> class using the specified download <paramref name="client"/>.
        /// </summary>
        /// <param name="client">The client used to download md5sum files from the remote source.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="client"/> is <see langword="null"/>.</exception>
        public AssemblyVerifier(HttpClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _client = client;
        }

        /// <summary>
        /// Gets a value that indicates whether the md5sum file has been downloaded.
        /// </summary>
        public bool ChecksumDownloaded
        {
            get 
            { 
                return _md5sum != null; 
            }
        }

        /// <summary>
        /// Attempts to verify the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to verify.</param>
        /// <param name="result"><see langword="true"/> if the aseembly is current and not corrupt; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the assembly was verified successfully; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Attempts to download the current md5sum file from the remote source.
        /// </summary>
        /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
        public bool TryDownloadChecksum()
        {
            const string MD5SumUrl = "https://www.deltaconnected.com/arcdps/x64/d3d11.dll.md5sum";
            
            try
            {
                using (Task<string> download = _client.GetStringAsync(MD5SumUrl))
                {
                    if (download.Wait(10000) && download.IsCompletedSuccessfully)
                    {
                        _md5sum = download.Result.Substring(0, 32);
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
