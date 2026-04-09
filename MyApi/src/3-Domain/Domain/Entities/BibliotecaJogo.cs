using Domain.Entities;
using Domain.Entities.Base;

namespace FCG.Domain.Entities
{
    public class BibliotecaJogo : BaseEntity
    {
        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public Guid JogoId { get; set; }
        public Jogo? Jogo { get; set; }
        public DateTime DataCompra { get; set; }
        public decimal PrecoPago { get; set; }
        public Guid? PromocaoId { get; set; }
        public Promocao? Promocao { get; set; }
    }
}
