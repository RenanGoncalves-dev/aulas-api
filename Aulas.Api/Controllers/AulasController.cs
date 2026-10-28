using System.Security.Claims;
using Aulas.Api.Comum;
using Aulas.Api.Dtos.Aulas;
using Aulas.Api.Entidades;
using Aulas.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aulas.Api.Controllers;

[ApiController]
[Route("api/aulas")]
[Authorize]
public class AulasController : ControllerBase
{
    private readonly IAulaMarcadaServico _aulaMarcadaServico;

    public AulasController(IAulaMarcadaServico aulaMarcadaServico)
    {
        _aulaMarcadaServico = aulaMarcadaServico;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AulaResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(CancellationToken ct = default)
    {
        if (!TentarObterIdentidade(out var usuarioId, out var papel))
        {
            return this.ParaProblem(Resultado.Falha(
                "Token de autenticação inválido ou sem identificador de usuário.",
                TipoErro.NaoAutorizado));
        }

        var resultado = await _aulaMarcadaServico.ListarAsync(usuarioId, papel, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AulaResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarAulaDto dto,
        CancellationToken ct = default)
    {
        if (!TentarObterIdentidade(out var usuarioId, out _))
        {
            return this.ParaProblem(Resultado.Falha(
                "Token de autenticação inválido ou sem identificador de usuário.",
                TipoErro.NaoAutorizado));
        }

        var resultado = await _aulaMarcadaServico.CriarAsync(usuarioId, dto, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return StatusCode(StatusCodes.Status201Created, resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        if (!TentarObterIdentidade(out var usuarioId, out var papel))
        {
            return this.ParaProblem(Resultado.Falha(
                "Token de autenticação inválido ou sem identificador de usuário.",
                TipoErro.NaoAutorizado));
        }

        var resultado = await _aulaMarcadaServico.CancelarAsync(id, usuarioId, papel, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return NoContent();
    }

    private bool TentarObterIdentidade(out int usuarioId, out string papel)
    {
        usuarioId = 0;
        papel = User.FindFirstValue(ClaimTypes.Role) ?? Usuario.PapelCliente;

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out usuarioId) && usuarioId > 0;
    }
}

