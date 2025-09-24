using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FintechPSP.UserService.Services;

public interface ICredentialsProtector
{
    string Encrypt(string plaintext, out string keyId);
    string Decrypt(string cipherText, string keyId);
}

public class AesCredentialsProtector : ICredentialsProtector
{
    private readonly byte[] _key;
    private readonly string _keyId;

    public AesCredentialsProtector(IConfiguration config)
    {
        var key = config["Encryption:CredentialsKey"] ?? config["Jwt:Key"];
        if (string.IsNullOrEmpty(key) || Encoding.UTF8.GetBytes(key).Length < 32)
        {
            // Pad/derive 32 bytes from provided key for AES-256
            var src = Encoding.UTF8.GetBytes(key ?? "dev-key");
            Array.Resize(ref src, 32);
            _key = src;
        }
        else
        {
            var raw = Encoding.UTF8.GetBytes(key);
            Array.Resize(ref raw, 32);
            _key = raw;
        }
        _keyId = string.IsNullOrEmpty(config["Encryption:CredentialsKey"]) ? "jwt" : "enc1";
    }

    public string Encrypt(string plaintext, out string keyId)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // Store IV + cipher, base64
        var payload = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, payload, aes.IV.Length, cipher.Length);
        keyId = _keyId;
        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText, string keyId)
    {
        var data = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        var iv = new byte[16];
        Buffer.BlockCopy(data, 0, iv, 0, 16);
        aes.IV = iv;
        var cipher = new byte[data.Length - 16];
        Buffer.BlockCopy(data, 16, cipher, 0, cipher.Length);
        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}

