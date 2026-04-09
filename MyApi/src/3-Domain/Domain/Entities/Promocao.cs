using Domain.Entities.Base;
using Domain.Enums;

namespace FCG.Domain.Entities
{
    public class Promocao : BaseEntity
    {
        public string? Nome { get; set; }
        public TipoDesconto TipoDesconto { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }

        // Relacionamento com Jogo
        public Guid IdJogo { get; set; }
        public Jogo? Jogo { get; set; }

        public ICollection<BibliotecaJogo>? BibliotecaJogos { get; set; }
    }
}
