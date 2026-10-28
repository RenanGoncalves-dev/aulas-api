using Aulas.Api.Comum;
using Aulas.Api.Dtos.Professores;
using Aulas.Api.Entidades;
using Aulas.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aulas.Api.Controllers;

[ApiController]
[Route("api/professores")]
public class ProfessoresController : ControllerBase
{
    private readonly IProfessorServico _professorServico;

    public ProfessoresController(IProfessorServico professorServico)
    {
        _professorServico = professorServico;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResultadoPaginado<ProfessorResposta>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 10,
        CancellationToken ct = default)
    {
        var resultado = await _professorServico.ListarAsync(busca, pagina, tamanho, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return Ok(resultado.Valor);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProfessorResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var resultado = await _professorServico.ObterPorIdAsync(id, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(typeof(ProfessorResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar(
        [FromBody] ProfessorCriarRequisicao requisicao,
        CancellationToken ct = default)
    {
        var resultado = await _professorServico.CriarAsync(requisicao, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(typeof(ProfessorResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        [FromRoute] int id,
        [FromBody] ProfessorAtualizarRequisicao requisicao,
        CancellationToken ct = default)
    {
        var resultado = await _professorServico.AtualizarAsync(id, requisicao, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return Ok(resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Usuario.PapelAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(
        [FromRoute] int id,
        CancellationToken ct = default)
    {
        var resultado = await _professorServico.ExcluirAsync(id, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return NoContent();
    }
}

