using System;
using System.IO;
using System.Security.Cryptography;

namespace ArcUpdater
{
    /// <summary>
    /// Encapsulates a backing <see cref="Stream"/> and provides methods to help verify and copy its contents.
    /// </summary>
    public class ArcAssembly : IDisposable
    {
        private Stream _baseStream;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArcAssembly"/> class using the specified backing <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="stream"/> is <see langword="null"/>.</exception>
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
        /// Releases all resources used by the underlying stream.
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

        /// <summary>
        /// Computes the MD5 hash of the underlying stream.
        /// </summary>
        /// <returns>The unformatted, lowercase, hexadecimal representation of the MD5 hash.</returns>
        /// <exception cref="ObjectDisposedException">The underlying stream is closed.</exception>
        public string ComputeChecksum()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            // Must reset position for proper ComputeHash result for downloaded stream.
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

        /// <summary>
        /// Reads the bytes of the underlying stream and writes them to the specified <paramref name="destination"/> stream.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the underlying stream will be copied.</param>
        /// <exception cref="ObjectDisposedException">The underlying stream is closed.</exception>
        public void CopyTo(Stream destination)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            // Must reset position before copying.
            _baseStream.Seek(0, SeekOrigin.Begin);
            _baseStream.CopyTo(destination);
        }
    }
}
