using System;
using System.IO;
using System.IO.Compression;
using SaveToolbox.Runtime.Core;

namespace SaveToolbox.Runtime.Serialization
{
	/// <summary>
	/// The Save Toolbox compression static class. Allows for the compression and decompression of byte[] data.
	/// </summary>
	public static class StbSaveCompression
	{
		/// <summary>
		/// Compresses a byte array if the compression is enabled.
		/// </summary>
		/// <param name="serializedData">The byte array data.</param>
		/// <param name="compressionType">The compression type that would like to be applied can be set to none for no compression.</param>
		/// <returns>The compressed byte array.</returns>
		public static byte[] Compress(byte[] serializedData, StbCompressionType compressionType)
		{
			switch (compressionType)
			{
				case StbCompressionType.None:
					break;
				case StbCompressionType.Gzip:
				{
					using (var stream = new MemoryStream())
					{
						using (var gzipStream = new GZipStream(stream, CompressionMode.Compress))
						{
							gzipStream.Write(serializedData, 0, serializedData.Length);
						}
						serializedData = stream.ToArray();
					}
					break;
				}
			}

			return serializedData;
		}

		/// <summary>
		/// Decompresses a byte array to it's original byte array format.
		/// </summary>
		/// <param name="serializedData">The compressed data.</param>
		/// <param name="compressionType">The type of compression that was originally applied so it can be handled accordingly.</param>
		/// <returns>The decompressed byte array.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static byte[] Decompress(byte[] serializedData, StbCompressionType compressionType)
		{
			switch (compressionType)
			{
				case StbCompressionType.None:
					break;
				case StbCompressionType.Gzip:
					using (var memoryStream = new MemoryStream(serializedData))
					{
						using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
						{
							using (var decompressedStream = new MemoryStream())
							{
								gZipStream.CopyTo(decompressedStream);
								serializedData = decompressedStream.ToArray();
							}
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return serializedData;
		}
	}
}
