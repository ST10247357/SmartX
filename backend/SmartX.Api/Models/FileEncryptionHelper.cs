using System.Security.Cryptography;

namespace SmartX.Api.Services;

// Encrypts uploaded files at rest using AES-256. The key is read from
// configuration (appsettings.json / environment variables / user secrets)
// rather than hardcoded in source, so it is never committed to Git.
public static class FileEncryptionHelper
{
    private static byte[]? _key;

    // Called once at startup from Program.cs. Hashes the configured string
    // with SHA-256 so any input length always yields a valid 32-byte AES-256 key.
    public static void Initialise(string configuredKey)
    {
        if (string.IsNullOrWhiteSpace(configuredKey))
            throw new InvalidOperationException("Encryption key is not configured. Set Encryption:Key via appsettings, an environment variable, or user secrets.");

        _key = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(configuredKey));
    }

    private static byte[] Key =>
        _key ?? throw new InvalidOperationException("FileEncryptionHelper.Initialise() was not called at startup.");

    public static async Task<byte[]> EncryptAsync(Stream inputStream)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV(); // random IV per file, prepended to the output so it can be decrypted later

        using var memoryStream = new MemoryStream();
        await memoryStream.WriteAsync(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            await inputStream.CopyToAsync(cryptoStream);
        }

        return memoryStream.ToArray();
    }

    public static byte[] Decrypt(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key = Key;

        var iv = new byte[aes.IV.Length];
        Array.Copy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var memoryStream = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var output = new MemoryStream();
        cryptoStream.CopyTo(output);
        return output.ToArray();
    }
}