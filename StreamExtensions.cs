using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace System.IO
{
	public static class StreamExtensions
	{
		public static Task WriteAsync(this Stream reqStream, byte[] buffer, int offset, int count)
		{
			return Task.Factory.FromAsync(reqStream.BeginWrite, reqStream.EndWrite, buffer, offset, count, null);
		}

		public static Stream AsBase64InputStream(this Stream stream, FromBase64TransformMode mode = FromBase64TransformMode.IgnoreWhiteSpaces)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			return new CryptoStream(stream, new FromBase64Transform(mode), CryptoStreamMode.Read);
		}

		public static Stream AsBase64OutputStream(this Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			return new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Write);
		}
	}
}