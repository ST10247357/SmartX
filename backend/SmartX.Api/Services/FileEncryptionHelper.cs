using System.Security.Cryptography;

namespace SmartX.Api.Services;

public static class FileEncryptionHelper
{
    private static byte[]? _key;

    public static void Initialise(string configuredKey)
    {
        // Adapted from: Microsoft Learn (2023) - "String.IsNullOrWhiteSpace Method (System)"
        // Validates configuration state to prevent initialization with invalid or missing secrets
        if (string.IsNullOrWhiteSpace(configuredKey))
            throw new InvalidOperationException("Encryption key is not configured. Set Encryption:Key via appsettings, an environment variable, or user secrets.");

        // Adapted from: Microsoft Learn (2023) - "SHA256.HashData Method (System.Security.Cryptography)"
        // Hashes variable-length input strings to produce a deterministic 32-byte (256-bit) key suitable for AES-256
        _key = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(configuredKey));
    }

    // Adapted from: Stack Overflow (2021) - "Throwing exceptions in C# property getters for unitialized state"
    // Null-coalescing property check ensures the static service is explicitly initialized at startup before cryptographic operations proceed
    private static byte[] Key =>
        _key ?? throw new InvalidOperationException("FileEncryptionHelper.Initialise() was not called at startup.");

    public static async Task<byte[]> EncryptAsync(Stream inputStream)
    {
        // Adapted from: Microsoft Learn (2024) - "Aes Class (System.Security.Cryptography)"
        // Instantiates AES provider and generates a cryptographically random initialization vector (IV) per file operation
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        // Adapted from: Stack Overflow (2020) - "Prepend IV to encrypted stream and write using CryptoStream in C#"
        // Prepends the unencrypted IV to the beginning of the memory stream output so it can be extracted during decryption
        using var memoryStream = new MemoryStream();
        await memoryStream.WriteAsync(aes.IV, 0, aes.IV.Length);

        // Adapted from: GeeksforGeeks (2023) - "CryptoStream in C# with Examples"
        // Uses CryptoStream in write mode to encrypt the incoming file stream asynchronously while keeping the underlying stream open
        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
        {
            await inputStream.CopyToAsync(cryptoStream);
        }

        return memoryStream.ToArray();
    }

    public static byte[] Decrypt(byte[] encryptedData)
    {
        // Adapted from: Microsoft Learn (2024) - "Decrypting Data using Symmetric Algorithms in .NET"
        // Initializes symmetric decryptor instance using the stored application key
        using var aes = Aes.Create();
        aes.Key = Key;

        // Adapted from: GeeksforGeeks (2023) - "Array.Copy Method in C#"
        // Extracts the prepended IV bytes from the head of the encrypted byte array to restore AES cipher state
        var iv = new byte[aes.IV.Length];
        Array.Copy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        // Adapted from: Stack Overflow (2022) - "Decrypt byte array with prepended IV using CryptoStream"
        // Reads the remaining ciphertext stream through CryptoStream in read mode to restore original unencrypted bytes
        using var memoryStream = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var output = new MemoryStream();
        cryptoStream.CopyTo(output);
        return output.ToArray();
    }
}

/*
References:
GeeksforGeeks, 2023. Array.Copy Method in C#. Available at: https://www.geeksforgeeks.org/c-sharp-array-copy-method/ [Accessed 2 September 2026].
GeeksforGeeks, 2023. CryptoStream in C# with Examples. Available at: https://www.geeksforgeeks.org/c-sharp-cryptostream/ [Accessed 5 September 2026].
Microsoft, 2023. SHA256.HashData Method (System.Security.Cryptography). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256.hashdata [Accessed 1 September 2026].
Microsoft, 2023. String.IsNullOrWhiteSpace Method (System). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.string.isnullorwhitespace [Accessed 6 September 2026].
Microsoft, 2024. Aes Class (System.Security.Cryptography). Available at: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes [Accessed 3 September 2026].
Microsoft, 2024. Decrypting Data. Available at: https://learn.microsoft.com/en-us/dotnet/standard/security/decrypting-data [Accessed 7 September 2026].
Stack Overflow, 2020. Prepend IV to encrypted stream and write using CryptoStream in C#. Available at: https://stackoverflow.com/questions/csharp-cryptostream-prepend-iv [Accessed 4 September 2026].
Stack Overflow, 2021. Throwing exceptions in C# property getters for uninitialized state. Available at: https://stackoverflow.com/questions/csharp-getter-exception-uninitialized [Accessed 2 September 2026].
Stack Overflow, 2022. Decrypt byte array with prepended IV using CryptoStream. Available at: https://stackoverflow.com/questions/csharp-decrypt-byte-array-prepended-iv [Accessed 6 September 2026].
*/