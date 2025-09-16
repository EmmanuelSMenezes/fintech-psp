using System.Data;

namespace FintechPSP.Shared.Infrastructure.Database;

/// <summary>
/// Factory para criar conexões com o banco de dados
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Cria uma nova conexão com o banco de dados
    /// </summary>
    IDbConnection CreateConnection();
    
    /// <summary>
    /// Cria uma nova conexão com o banco de dados específico
    /// </summary>
    IDbConnection CreateConnection(string connectionString);
}
