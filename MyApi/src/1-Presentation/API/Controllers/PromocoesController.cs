using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PromocoesController : ControllerBase
    {
        private readonly IPromocaoService _promocaoService;

        public PromocoesController(IPromocaoService promocaoService)
        {
            _promocaoService = promocaoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<PromocaoDTO>>> GetAllPromocoes(CancellationToken cancellationToken)
        {
            var result = await _promocaoService.GetAllPromocoesAsync(cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PromocaoDTO>> GetPromocaoById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _promocaoService.GetPromocaoByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PromocaoDTO>> CreatePromocao(CreatePromocaoDTO dto, CancellationToken cancellationToken)
        {
            var result = await _promocaoService.CreatePromocaoAsync(dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetPromocaoById), new { id = result.Data!.Id }, result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePromocao(Guid id, UpdatePromocaoDTO dto, CancellationToken cancellationToken)
        {
            var result = await _promocaoService.UpdatePromocaoAsync(id, dto, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePromocao(Guid id, CancellationToken cancellationToken)
        {
            var result = await _promocaoService.DeletePromocaoAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, new { message = result.ErrorMessage });

            return NoContent();
        }
    }
}
