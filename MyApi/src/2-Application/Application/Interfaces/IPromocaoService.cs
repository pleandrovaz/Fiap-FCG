using Application.DTOs;

namespace Application.Interfaces
{
    public interface IPromocaoService
    {
        Task<Result<IEnumerable<PromocaoDTO>>> GetAllPromocoesAsync(CancellationToken cancellationToken = default);
        Task<Result<PromocaoDTO>> GetPromocaoByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<PromocaoDTO>> CreatePromocaoAsync(CreatePromocaoDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> UpdatePromocaoAsync(Guid id, UpdatePromocaoDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeletePromocaoAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
