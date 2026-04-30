using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DevStack.Infrastructure.Services;

public class AesSecretService : ISecretService
{
    private const string VersionPrefix = "v1:";
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesSecretService(string? secretKey = null)
    {
        if (string.IsNullOrEmpty(secretKey))
        {
            secretKey = GetDpapiSecret();
        }

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("No secret key available: set DEVSTACK_SECRET_KEY or ensure DPAPI is available.");

        var salt = GetSalt(secretKey);
        _key = Rfc2898DeriveBytes.Pbkdf2(secretKey, salt, 10000, HashAlgorithmName.SHA256, 32);
        _iv = Rfc2898DeriveBytes.Pbkdf2(secretKey, salt, 10000, HashAlgorithmName.SHA256, 16);
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var writer = new StreamWriter(cs);

        writer.Write(plaintext);
        writer.Close();

        var encryptedBytes = ms.ToArray();
        return $"{VersionPrefix}{Convert.ToBase64String(encryptedBytes)}";
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return string.Empty;

        if (!ciphertext.StartsWith(VersionPrefix))
            throw new InvalidOperationException("Invalid encrypted format");

        var base64Data = ciphertext[VersionPrefix.Length..];
        var encryptedBytes = Convert.FromBase64String(base64Data);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(encryptedBytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);

        return reader.ReadToEnd();
    }

    private static byte[] GetSalt(string secretKey)
    {
        var saltSource = $"devstack-salt-{secretKey}";
        return Encoding.UTF8.GetBytes(saltSource);
    }

    private static string? GetDpapiSecret()
    {
        return Environment.GetEnvironmentVariable("DEVSTACK_SECRET_KEY");
    }
}
