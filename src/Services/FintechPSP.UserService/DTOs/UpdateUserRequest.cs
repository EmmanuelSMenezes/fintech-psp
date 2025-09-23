using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.DTOs
{
    public class UpdateUserRequest
    {
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Role deve ter no máximo 50 caracteres")]
        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string? Password { get; set; }
    }
}
