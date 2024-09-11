using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SaveToolbox.Runtime.Core;

namespace SaveToolbox.Runtime.Serialization
{
	/// <summary>
	/// The Save Toolbox encryption static class. Allows for the encryption and decompression of byte[] data.
	/// </summary>
	public static class StbSaveEncryption
	{
		/// <summary>
		/// Encrypts a byte array of data.
		/// </summary>
		/// <param name="data">The byte array of data to be encrypted.</param>
		/// <param name="encryptionType">The type of encryption to be applied.</param>
		/// <param name="key">The key to be used if it is required. </param>
		/// <param name="iv">The iv to be used if it is Aes encryption.</param>
		/// <returns>The encrypted data.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Argument of range exception if there is an undefined encryption type.</exception>
		public static byte[] Encrypt(byte[] data, StbEncryptionType encryptionType, string key, string iv = "")
		{
			switch (encryptionType)
			{
				case StbEncryptionType.None:
					break;
				case StbEncryptionType.Xor:
					var encryptedData = new byte[data.Length];
					var encryptionKey = key;
					for (var i = 0; i < data.Length; i++)
					{
						encryptedData[i] = (byte)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
					}
					data = encryptedData;
					break;
				case StbEncryptionType.Aes:
					using (var aesManaged = new AesManaged())
					{
						aesManaged.Key = GetValidKey(key);
						aesManaged.IV = ConvertStringToBytes(iv);

						using (var memoryStream = new MemoryStream())
						{
							using (var cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateEncryptor(), CryptoStreamMode.Write))
							{
								cryptoStream.Write(data, 0, data.Length);
								cryptoStream.FlushFinalBlock();
							}
							data = memoryStream.ToArray();
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(encryptionType), encryptionType, null);
			}

			return data;
		}

		/// <summary>
		/// Decrypts byte array data.
		/// </summary>
		/// <param name="data">The byte array data to be decrypted.</param>
		/// <param name="encryptionType">The type of encryption originally used so it can be handled accordingly.</param>
		/// <param name="key">The key used for encryption.</param>
		/// <param name="iv">The iv used for encryption if the encryption type was Aes.</param>
		/// <returns>The decrypted data.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Argument of range exception if there is an undefined encryption type.</exception>
		public static byte[] Decrypt(byte[] data, StbEncryptionType encryptionType, string key, string iv= "")
		{
			switch (encryptionType)
			{
				case StbEncryptionType.None:
					break;
				case StbEncryptionType.Xor:
					var encryptedData = new byte[data.Length];
					var encryptionKey = key;
					for (var i = 0; i < data.Length; i++)
					{
						encryptedData[i] = (byte)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
					}
					data = encryptedData;
					break;
				case StbEncryptionType.Aes:
					using (var aesManaged = new AesManaged())
					{
						aesManaged.Key = GetValidKey(key);
						aesManaged.IV = ConvertStringToBytes(iv);

						using (var memoryStream = new MemoryStream())
						{
							using (var cryptoStream = new CryptoStream(memoryStream, aesManaged.CreateDecryptor(), CryptoStreamMode.Write))
							{
								cryptoStream.Write(data, 0, data.Length);
								cryptoStream.FlushFinalBlock();
							}
							data = memoryStream.ToArray();
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(encryptionType), encryptionType, null);
			}

			return data;
		}

		private static byte[] GetValidKey(string password)
		{
			var sha256 = SHA256.Create();
			return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
		}

		public static byte[] ConvertStringToBytes(string stringValue)
		{
			return Convert.FromBase64String(stringValue);
		}

		public static string ConvertBytesToString(byte[] byteArray)
		{
			return Convert.ToBase64String(byteArray);
		}
	}
}
