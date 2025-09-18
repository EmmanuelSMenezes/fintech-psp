using FintechPSP.AuthService.DTOs;
using MediatR;

namespace FintechPSP.AuthService.Commands;

/// <summary>
/// Comando para login de usuário
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
