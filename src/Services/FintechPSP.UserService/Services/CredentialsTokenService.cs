using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Services;

/// <summary>
/// Implementação do serviço de tokenização de credenciais
/// </summary>
public class CredentialsTokenService : ICredentialsTokenService
{
    private readonly ICredentialsTokenRepository _tokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CredentialsTokenService> _logger;
    private readonly string _encryptionKey;

    public CredentialsTokenService(
        ICredentialsTokenRepository tokenRepository,
        IConfiguration configuration,
        ILogger<CredentialsTokenService> logger)
    {
        _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Em produção, usar Azure Key Vault ou similar
        _encryptionKey = _configuration["CredentialsEncryption:Key"] ?? "fintech_psp_credentials_key_2024_very_secure";
    }

    public async Task<string> TokenizeCredentialsAsync(Guid contaId, ContaCredentials credentials)
    {
        try
        {
            var tokenId = Guid.NewGuid().ToString("N");
            var credentialsJson = JsonSerializer.Serialize(credentials);
            var encryptedCredentials = EncryptString(credentialsJson);

            var token = new ContaCredentialsToken
            {
                TokenId = tokenId,
                ContaId = contaId,
                EncryptedCredentials = encryptedCredentials,
                CreatedAt = DateTime.UtcNow
            };

            await _tokenRepository.CreateAsync(token);
            
            _logger.LogInformation("Credenciais tokenizadas com sucesso para conta {ContaId}", contaId);
            return tokenId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao tokenizar credenciais para conta {ContaId}", contaId);
            throw;
        }
    }

    public async Task<ContaCredentials?> GetCredentialsAsync(string tokenId)
    {
        try
        {
            var token = await _tokenRepository.GetByTokenIdAsync(tokenId);
            if (token == null)
            {
                _logger.LogWarning("Token não encontrado: {TokenId}", tokenId);
                return null;
            }

            var decryptedJson = DecryptString(token.EncryptedCredentials);
            var credentials = JsonSerializer.Deserialize<ContaCredentials>(decryptedJson);
            
            return credentials;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar credenciais do token {TokenId}", tokenId);
            throw;
        }
    }

    public async Task<bool> UpdateCredentialsAsync(string tokenId, ContaCredentials credentials)
    {
        try
        {
            var token = await _tokenRepository.GetByTokenIdAsync(tokenId);
            if (token == null)
            {
                _logger.LogWarning("Token não encontrado para atualização: {TokenId}", tokenId);
                return false;
            }

            var credentialsJson = JsonSerializer.Serialize(credentials);
            var encryptedCredentials = EncryptString(credentialsJson);

            token.EncryptedCredentials = encryptedCredentials;
            token.UpdatedAt = DateTime.UtcNow;

            await _tokenRepository.UpdateAsync(token);
            
            _logger.LogInformation("Credenciais atualizadas com sucesso para token {TokenId}", tokenId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar credenciais do token {TokenId}", tokenId);
            throw;
        }
    }

    public async Task<bool> RemoveCredentialsAsync(string tokenId)
    {
        try
        {
            var result = await _tokenRepository.DeleteAsync(tokenId);
            
            if (result)
            {
                _logger.LogInformation("Token removido com sucesso: {TokenId}", tokenId);
            }
            else
            {
                _logger.LogWarning("Token não encontrado para remoção: {TokenId}", tokenId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover token {TokenId}", tokenId);
            throw;
        }
    }

    private string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    private string DecryptString(string cipherText)
    {
        var cipherBytes = Convert.FromBase64String(cipherText);
        
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
        
        var iv = new byte[16];
        var encrypted = new byte[cipherBytes.Length - 16];
        
        Array.Copy(cipherBytes, 0, iv, 0, 16);
        Array.Copy(cipherBytes, 16, encrypted, 0, encrypted.Length);
        
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
