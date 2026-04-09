using Domain.Entities.Base;
using Domain.Enums;
using FCG.Domain.Entities;

namespace Domain.Entities
{
    public class Usuario : BaseEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }
        public PerfilUsuario Perfil { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }

        // Relacionamento
        public ICollection<BibliotecaJogo>? Biblioteca { get; set; }
    }
}
