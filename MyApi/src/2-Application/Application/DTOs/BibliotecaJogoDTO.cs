using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BibliotecaJogoDTO
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public Guid JogoId { get; set; }
        public string? JogoNome { get; set; }
        public DateTime DataCompra { get; set; }
        public decimal PrecoPago { get; set; }
        public Guid? PromocaoId { get; set; }
    }

    public class CreateBibliotecaJogoDTO
    {
        [Required(ErrorMessage = "JogoId is required")]
        public Guid JogoId { get; set; }

        public Guid? PromocaoId { get; set; }
    }
}
