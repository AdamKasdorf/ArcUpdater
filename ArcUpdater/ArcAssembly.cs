using System;
using System.IO;
using System.Security.Cryptography;

namespace ArcUpdater
{
    public class ArcAssembly : IDisposable
    {
        private Stream _stream;
        private bool _disposed;

        public ArcAssembly(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _stream = stream;
        }

        ~ArcAssembly()
        {
            Dispose(false);
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

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            _disposed = true;
        }

        public string ComputeChecksum()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            _stream.Seek(0, SeekOrigin.Begin);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(_stream);
#if NET5_0_OR_GREATER
                return Convert.ToHexString(hash).ToLowerInvariant(); 
#else
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
#endif
            }
        }

        public void WriteToFile(string filePath)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            _stream.Seek(0, SeekOrigin.Begin);

            using (FileStream file = File.Create(filePath))
            {
                _stream.CopyTo(file);
            }
        }

        public static ArcAssembly Open(string filePath, FileMode mode)
        {
            Stream stream = File.Open(filePath, mode, FileAccess.ReadWrite);
            return new ArcAssembly(stream);
        }
    }
}
