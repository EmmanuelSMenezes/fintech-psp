using System;

class Program
{
    static void Main()
    {
        string password = "admin123";
        
        // Gerar hash usando BCrypt.Net-Next
        string hash = BCrypt.Net.BCrypt.HashPassword(password, 10);
        Console.WriteLine($"Hash gerado: {hash}");
        
        // Verificar se funciona
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine($"Verificação: {isValid}");
    }
}
