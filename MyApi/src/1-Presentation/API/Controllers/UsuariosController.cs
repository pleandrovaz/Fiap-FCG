using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDTO>>> TodosUsuarios(CancellationToken cancellationToken)
        {
            var callerRole = User.IsInRole("Admin") ? "Admin" : "User";
            var result = await _usuarioService.GetAllUsuariosAsync(callerRole, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDTO>> UsuarioPorId(Guid id, CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
                return Unauthorized();

            if (!User.IsInRole("Admin") && currentUserId != id)
                return Forbid();

            var callerRole = User.IsInRole("Admin") ? "Admin" : "User";
            var result = await _usuarioService.GetUsuarioByIdAsync(id, callerRole, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarUsuario(Guid id, RegisterUsuarioDTO usuarioDto, CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
                return Unauthorized();

            if (!User.IsInRole("Admin") && currentUserId != id)
                return Forbid();

            var result = await _usuarioService.UpdateUsuarioAsync(id, usuarioDto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUsuario(Guid id, CancellationToken cancellationToken)
        {
            var result = await _usuarioService.DeleteUsuarioAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }
    }
}
