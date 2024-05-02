using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class SimplerAES
{
	private static byte[] key = new byte[32]
	{
		223, 14, 113, 21, 213, 84, 36, 143, 36, 1,
		218, 190, 238, 13, 132, 209, 211, 214, 22, 157,
		123, 99, 101, 39, 33, 99, 12, 92, 111, 131,
		171, 92
	};

	private static byte[] vector = new byte[16]
	{
		6, 235, 131, 215, 33, 213, 13, 119, 131, 143,
		113, 221, 29, 12, 42, 156
	};

	private ICryptoTransform encryptor;

	private ICryptoTransform decryptor;

	private UTF8Encoding encoder;

	public SimplerAES()
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		encryptor = rijndaelManaged.CreateEncryptor(key, vector);
		decryptor = rijndaelManaged.CreateDecryptor(key, vector);
		encoder = new UTF8Encoding();
	}

	public string Encrypt(string unencrypted)
	{
		return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
	}

	public string Decrypt(string encrypted)
	{
		return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
	}

	public byte[] Encrypt(byte[] buffer)
	{
		return Transform(buffer, encryptor);
	}

	public byte[] Decrypt(byte[] buffer)
	{
		return Transform(buffer, decryptor);
	}

	protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			cryptoStream.Write(buffer, 0, buffer.Length);
		}
		return memoryStream.ToArray();
	}
}
