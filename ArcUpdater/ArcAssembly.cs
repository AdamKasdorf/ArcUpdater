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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            GC.SuppressFinalize(this);
        }

        public string ComputeChecksum()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ArcAssembly));
            }

            _stream.Position = 0;

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

            _stream.Position = 0;

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
