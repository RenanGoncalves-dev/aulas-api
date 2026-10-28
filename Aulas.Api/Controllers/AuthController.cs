using Aulas.Api.Comum;
using Aulas.Api.Dtos.Auth;
using Aulas.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aulas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthServico _authServico;

    public AuthController(IAuthServico authServico)
    {
        _authServico = authServico;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResposta), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroRequisicao requisicao,
        CancellationToken ct)
    {
        var resultado = await _authServico.RegistrarAsync(requisicao, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return StatusCode(StatusCodes.Status201Created, resultado.Valor);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResposta), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequisicao requisicao,
        CancellationToken ct)
    {
        var resultado = await _authServico.LoginAsync(requisicao, ct);

        if (!resultado.Sucesso)
        {
            return this.ParaProblem(resultado);
        }

        return Ok(resultado.Valor);
    }
}

