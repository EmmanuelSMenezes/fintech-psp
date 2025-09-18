namespace FintechPSP.AuthService.DTOs;

/// <summary>
/// Response do login de usuário
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Token de acesso JWT
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do token (sempre "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiração em segundos
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Dados do usuário logado
    /// </summary>
    public UserInfo User { get; set; } = new();
}

/// <summary>
/// Informações do usuário logado
/// </summary>
public class UserInfo
{
    /// <summary>
    /// ID do usuário
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role do usuário
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Se é usuário master
    /// </summary>
    public bool IsMaster { get; set; }
}
