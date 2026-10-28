using Microsoft.AspNetCore.Mvc;

namespace Aulas.Api.Comum;

public static class ControllerExtensions
{
    public static ObjectResult ParaProblem(this ControllerBase controller, Resultado resultado)
    {
        var (statusCode, titulo) = resultado.TipoErro switch
        {
            TipoErro.Validacao => (StatusCodes.Status400BadRequest, "Erro de validação"),
            TipoErro.NaoAutorizado => (StatusCodes.Status401Unauthorized, "Não autorizado"),
            TipoErro.Proibido => (StatusCodes.Status403Forbidden, "Acesso negado"),
            TipoErro.NaoEncontrado => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            TipoErro.Conflito => (StatusCodes.Status409Conflict, "Conflito de regra de negócio"),
            _ => (StatusCodes.Status400BadRequest, "Requisição inválida")
        };

        return controller.Problem(
            title: titulo,
            detail: resultado.Erro,
            statusCode: statusCode);
    }
}
