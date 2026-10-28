using Aulas.Api.Comum;
using Aulas.Api.Dtos.Aulas;

namespace Aulas.Api.Servicos;

public interface IAulaMarcadaServico
{
    Task<Resultado<AulaResposta>> CriarAsync(
        int usuarioId,
        CriarAulaDto dto,
        CancellationToken ct = default);

    Task<Resultado<IReadOnlyList<AulaResposta>>> ListarAsync(
        int usuarioId,
        string papel,
        CancellationToken ct = default);

    Task<Resultado> CancelarAsync(
        int aulaId,
        int usuarioId,
        string papel,
        CancellationToken ct = default);
}

