using Aulas.Api.Comum;
using Aulas.Api.Dtos.Auth;

namespace Aulas.Api.Servicos;

public interface IAuthServico
{
    Task<Resultado<AuthResposta>> RegistrarAsync(RegistroRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<AuthResposta>> LoginAsync(LoginRequisicao requisicao, CancellationToken ct = default);
}
