using Domain.Entities.Base;

namespace FCG.Domain.Entities
{
    public class Jogo : BaseEntity
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }

        // Relacionamentos
        public ICollection<BibliotecaJogo>? Biblioteca { get; set; }
        public ICollection<Promocao>? Promocoes { get; set; }
    }
}