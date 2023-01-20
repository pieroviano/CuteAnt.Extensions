#if NET35
using System;
using System.IO;

namespace Microsoft.Extensions.Configuration.Xml
{
    internal static class StreamExtension
    {
        public static void CopyTo(this Stream source, Stream destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!source.CanRead && !source.CanWrite)
            {
                throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
            }

            if (!destination.CanRead && !destination.CanWrite)
            {
                throw new ObjectDisposedException(nameof(destination), "ObjectDisposed_StreamClosed");
            }

            if (!source.CanRead)
            {
                throw new NotSupportedException("NotSupported_UnreadableStream");
            }

            if (!destination.CanWrite)
            {
                throw new NotSupportedException("NotSupported_UnwritableStream");
            }

            source.InternalCopyTo(destination, 81920);
        }

        private static void InternalCopyTo(this Stream source, Stream destination, int bufferSize)
        {
            var buffer = new byte[bufferSize];
            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }
    }
}
#endif