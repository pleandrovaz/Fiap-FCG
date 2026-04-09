using Application.DTOs;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<Result<IEnumerable<UsuarioDTO>>> GetAllUsuariosAsync(string? callerRole = null, CancellationToken cancellationToken = default);
        Task<Result<UsuarioDTO>> GetUsuarioByIdAsync(Guid id, string? callerRole = null, CancellationToken cancellationToken = default);
        Task<Result<UsuarioDTO>> CreateUsuarioAsync(RegisterUsuarioDTO registerDto, CancellationToken cancellationToken = default);
        Task<Result<LoginResponseDTO>> AuthenticateAsync(LoginDTO loginDto, CancellationToken cancellationToken = default);
        Task<Result<bool>> UpdateUsuarioAsync(Guid id, RegisterUsuarioDTO usuarioDto, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteUsuarioAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<ForgotPasswordResponseDTO>> ForgotPasswordAsync(ForgotPasswordDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDTO dto, CancellationToken cancellationToken = default);
    }
}
