using Application.DTOs;
using Application.Interfaces;
using Application.Resources;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<IEnumerable<UsuarioDTO>>> GetAllUsuariosAsync(string? callerRole = null, CancellationToken cancellationToken = default)
        {
            var usuarios = await _usuarioRepository.GetAllAsync(cancellationToken);

            if (callerRole != "Admin")
                usuarios = usuarios.Where(u => u.Role != "Admin");

            var result = usuarios.Select(u => new UsuarioDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            });

            return Result<IEnumerable<UsuarioDTO>>.Success(result);
        }

        public async Task<Result<UsuarioDTO>> GetUsuarioByIdAsync(Guid id, string? callerRole = null, CancellationToken cancellationToken = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);

            if (usuario == null)
                return Result<UsuarioDTO>.Failure(Messages.UsuarioNaoEncontrado, 404);

            if (callerRole != "Admin" && usuario.Role == "Admin")
                return Result<UsuarioDTO>.Failure(Messages.UsuarioSemPermissao, 403);

            return Result<UsuarioDTO>.Success(new UsuarioDTO
            {
                Email = usuario.Email,
                Id = usuario.Id,
                Name = usuario.Name,
                Role = usuario.Role
            });
        }

        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PasswordRegex = new(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$", RegexOptions.Compiled);

        public async Task<Result<UsuarioDTO>> CreateUsuarioAsync(RegisterUsuarioDTO registerDto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(registerDto.Email) || !EmailRegex.IsMatch(registerDto.Email))
                return Result<UsuarioDTO>.Failure(Messages.EmailFormatoInvalido, 400);

            if (string.IsNullOrWhiteSpace(registerDto.Password) || !PasswordRegex.IsMatch(registerDto.Password))
                return Result<UsuarioDTO>.Failure(Messages.SenhaInsegura, 400);

            var existingUsuario = await _usuarioRepository.GetByEmailAsync(registerDto.Email, cancellationToken);
            if (existingUsuario != null)
                return Result<UsuarioDTO>.Failure(Messages.EmailJaExiste, 409);

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Name = registerDto.Name,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _usuarioRepository.AddAsync(usuario, cancellationToken);

            return Result<UsuarioDTO>.Success(new UsuarioDTO { Id = usuario.Id }, 201);
        }

        public async Task<Result<LoginResponseDTO>> AuthenticateAsync(LoginDTO loginDto, CancellationToken cancellationToken = default)
        {
            var passwordHash = HashPassword(loginDto.Password);
            var usuario = await _usuarioRepository.GetByEmailAndPasswordAsync(loginDto.Email, passwordHash, cancellationToken);

            if (usuario == null)
                return Result<LoginResponseDTO>.Failure(Messages.EmailOuSenhaInvalidos, 401);

            var token = _tokenService.GenerateToken(usuario);

            return Result<LoginResponseDTO>.Success(new LoginResponseDTO
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            });
        }

        public async Task<Result<bool>> UpdateUsuarioAsync(Guid id, RegisterUsuarioDTO usuarioDto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.Email) || !EmailRegex.IsMatch(usuarioDto.Email))
                return Result<bool>.Failure(Messages.EmailFormatoInvalido, 400);

            if (!string.IsNullOrWhiteSpace(usuarioDto.Password) && !PasswordRegex.IsMatch(usuarioDto.Password))
                return Result<bool>.Failure(Messages.SenhaInsegura, 400);

            var usuario = await _usuarioRepository.GetByIdAsync(id, cancellationToken);
            if (usuario == null)
                return Result<bool>.Failure(Messages.UsuarioNaoEncontrado, 404);

            usuario.Name = usuarioDto.Name;
            usuario.Email = usuarioDto.Email;
            usuario.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(usuarioDto.Password))
                usuario.PasswordHash = HashPassword(usuarioDto.Password);

            await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteUsuarioAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var deleted = await _usuarioRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return Result<bool>.Failure(Messages.UsuarioNaoEncontrado, 404);

            return Result<bool>.Success(true);
        }

        public async Task<Result<ForgotPasswordResponseDTO>> ForgotPasswordAsync(ForgotPasswordDTO dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
                return Result<ForgotPasswordResponseDTO>.Failure(Messages.EmailFormatoInvalido, 400);

            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (usuario == null)
                return Result<ForgotPasswordResponseDTO>.Failure(Messages.UsuarioNaoEncontrado, 404);

            var resetToken = GenerateResetToken();
            usuario.PasswordResetToken = resetToken;
            usuario.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
            usuario.UpdatedAt = DateTime.UtcNow;

            await _usuarioRepository.UpdateAsync(usuario, cancellationToken);

            // Em produção, o token seria enviado por e-mail e não retornado na resposta.
            return Result<ForgotPasswordResponseDTO>.Success(new ForgotPasswordResponseDTO
            {
                Message = Messages.TokenRecuperacaoEnviado,
                ResetToken = resetToken
            });
        }

        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDTO dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !EmailRegex.IsMatch(dto.Email))
                return Result<bool>.Failure(Messages.EmailFormatoInvalido, 400);

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || !PasswordRegex.IsMatch(dto.NewPassword))
                return Result<bool>.Failure(Messages.SenhaInsegura, 400);

            var usuario = await _usuarioRepository.GetByEmailAsync(dto.Email, cancellationToken);
            if (usuario == null)
                return Result<bool>.Failure(Messages.UsuarioNaoEncontrado, 404);

            if (usuario.PasswordResetToken != dto.Token ||
                usuario.PasswordResetTokenExpiration == null ||
                usuario.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                return Result<bool>.Failure(Messages.TokenInvalidoOuExpirado, 400);
            }

            usuario.PasswordHash = HashPassword(dto.NewPassword);
            usuario.PasswordResetToken = null;
            usuario.PasswordResetTokenExpiration = null;
            usuario.UpdatedAt = DateTime.UtcNow;

            await _usuarioRepository.UpdateAsync(usuario, cancellationToken);

            return Result<bool>.Success(true);
        }

        private static string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
