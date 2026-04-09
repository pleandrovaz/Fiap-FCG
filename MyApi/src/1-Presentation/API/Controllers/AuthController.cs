using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UsuarioDTO>> Register(RegisterUsuarioDTO registerDto, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.CreateUsuarioAsync(registerDto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetUsuario), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO loginDto, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.AuthenticateAsync(loginDto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario(Guid id, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.GetUsuarioByIdAsync(id, callerRole: null, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        /// <summary>
        /// Solicita a recuperação de senha. Gera um token de reset.
        /// </summary>
        [HttpPost("recuperar-senha")]
        public async Task<ActionResult<ForgotPasswordResponseDTO>> RecuperarSenha(ForgotPasswordDTO dto, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.ForgotPasswordAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        /// <summary>
        /// Redefine a senha utilizando o token de recuperação.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.ResetPasswordAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(new { message = "Senha alterada com sucesso" });
        }
    }
}
