using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class JogosController : ControllerBase
    {
        private readonly IJogoService _jogoService;

        public JogosController(IJogoService jogoService)
        {
            _jogoService = jogoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<JogoDTO>>> TodososJogos(CancellationToken cancellationToken)
        {
            var result = await _jogoService.GetAllJogosAsync(cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<JogoDTO>> GetJogoById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _jogoService.GetJogoByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JogoDTO>> NovoJogo(CreateJogoDTO dto, CancellationToken cancellationToken)
        {
            var result = await _jogoService.CreateJogoAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetJogoById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarJogo(Guid id, UpdateJogoDTO dto, CancellationToken cancellationToken)
        {
            var result = await _jogoService.UpdateJogoAsync(id, dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteJogo(Guid id, CancellationToken cancellationToken)
        {
            var result = await _jogoService.DeleteJogoAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }
    }
}
