using System;

class Program
{
    static void Main()
    {
        string password = "admin123";
        string hash = "$2a$11$8K1p/a0dRcK2P1cyPa2YveJ4kzY4rBGiVGOVvD6bnVXK8QXdtqm4.";
        
        // Criar um novo hash para comparar
        string newHash = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Nova hash: {newHash}");
        
        // Verificar se a senha bate com o hash existente
        bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        Console.WriteLine($"Senha válida com hash existente: {isValid}");
        
        // Verificar se a senha bate com o novo hash
        bool isValidNew = BCrypt.Net.BCrypt.Verify(password, newHash);
        Console.WriteLine($"Senha válida com novo hash: {isValidNew}");
    }
}
