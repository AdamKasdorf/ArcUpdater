using System;
using System.IO;
using System.Security.Cryptography;

namespace ArcUpdater
{
    public class ArcAssembly : IDisposable
    {
        private Stream _baseStream;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArcAssembly"/> class using the specified backing <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The underlying <see cref="Stream"/> object.</param>
        /// <exception cref="ArgumentNullException">
        /// The specified <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public ArcAssembly(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            
            _baseStream = stream;
        }

        ~ArcAssembly()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases all resources used by the underlying <see cref="Stream"/>.
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

            if (_baseStream != null)
            {
                _baseStream.Dispose();
                _baseStream = null;
            }

            _disposed = true;
        }

        public string ComputeChecksum()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            _baseStream.Seek(0, SeekOrigin.Begin);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(_baseStream);
#if NET5_0_OR_GREATER
                return Convert.ToHexString(hash).ToLowerInvariant(); 
#else
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
#endif
            }
        }

        public void CopyTo(Stream stream)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            _baseStream.Seek(0, SeekOrigin.Begin);
            _baseStream.CopyTo(stream);
        }
    }
}
