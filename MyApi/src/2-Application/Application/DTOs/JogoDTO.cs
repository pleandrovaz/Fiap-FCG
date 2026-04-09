using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class JogoDTO
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; }
    }

    public class CreateJogoDTO
    {
        [Required(ErrorMessage = "Nome obrigatório")]
        [StringLength(200, MinimumLength = 2)]
        public string? Nome { get; set; }

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Preco is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço precisa ser maior que zero")]
        public decimal Preco { get; set; }
    }

    public class UpdateJogoDTO
    {
        [Required(ErrorMessage = "Nome Obrigatorio")]
        [StringLength(200, MinimumLength = 2)]
        public string? Nome { get; set; }

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Preco is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preco precisa ser maior que zero")]
        public decimal Preco { get; set; }

        public bool Ativo { get; set; }
    }
}
