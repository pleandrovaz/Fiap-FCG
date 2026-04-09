using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class PromocaoDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public Guid IdJogo { get; set; }
        public string? JogoNome { get; set; }
        public TipoDesconto TipoDesconto { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public bool Ativa { get; set; }
    }

    public class CreatePromocaoDTO
    {
        [Required(ErrorMessage = "Nome Obrigatorio")]
        [StringLength(200, MinimumLength = 2)]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "IdJogo Obrigatorio")]
        public Guid IdJogo { get; set; }

        [Required(ErrorMessage = "TipoDesconto Obrigatorio")]
        public TipoDesconto TipoDesconto { get; set; }

        [Required(ErrorMessage = "Valor Obrigatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor precisa ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "DataInicio Obrigatorio")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "DataFim Obrigatorio")]
        public DateTime DataFim { get; set; }
    }

    public class UpdatePromocaoDTO
    {
        [Required(ErrorMessage = "Nome Obrigatorio")]
        [StringLength(200, MinimumLength = 2)]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "IdJogo is required")]
        public Guid IdJogo { get; set; }

        [Required(ErrorMessage = "TipoDesconto is required")]
        public TipoDesconto TipoDesconto { get; set; }

        [Required(ErrorMessage = "Valor Obrigatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor precisa ser maior zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "DataInicio is Obrigatorio")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "DataFim is Obrigatorio")]
        public DateTime DataFim { get; set; }

        public bool? Ativa { get; set; }
    }
}
