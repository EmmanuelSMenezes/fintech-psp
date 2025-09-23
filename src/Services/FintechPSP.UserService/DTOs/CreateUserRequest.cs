using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.DTOs
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
        public string Email { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Role deve ter no máximo 50 caracteres")]
        public string? Role { get; set; } = "cliente";

        public bool? IsActive { get; set; } = true;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string? Password { get; set; }
    }
}
