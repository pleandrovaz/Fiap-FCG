using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BibliotecaJogosController : ControllerBase
    {
        private readonly IBibliotecaJogoService _bibliotecaService;

        public BibliotecaJogosController(IBibliotecaJogoService bibliotecaService)
        {
            _bibliotecaService = bibliotecaService;
        }
        
        /// <summary>
        /// jogos do usuario logado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BibliotecaJogoDTO>>> GetMinhaBiblioteca(CancellationToken cancellationToken)
        {
            var inicioCorrelation = HttpContext.Items["RequestStartTime"];
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
                return Unauthorized();

            var result = await _bibliotecaService.GetBibliotecaByUsuarioIdAsync(currentUserId, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            var finalCorrelation = HttpContext.Items["RequestStartTime"];

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BibliotecaJogoDTO>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bibliotecaService.GetByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<BibliotecaJogoDTO>> AddJogo(CreateBibliotecaJogoDTO dto, CancellationToken cancellationToken)
        {
            var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId))
                return Unauthorized();

            var result = await _bibliotecaService.AddJogoToBibliotecaAsync(currentUserId, dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveJogo(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bibliotecaService.RemoveJogoFromBibliotecaAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }
    }
}
