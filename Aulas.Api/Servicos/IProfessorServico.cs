using Aulas.Api.Comum;
using Aulas.Api.Dtos.Professores;

namespace Aulas.Api.Servicos;

public interface IProfessorServico
{
    Task<Resultado<ResultadoPaginado<ProfessorResposta>>> ListarAsync(
        string? busca = null,
        int pagina = 1,
        int tamanho = 10,
        CancellationToken ct = default);

    Task<Resultado<ProfessorResposta>> ObterPorIdAsync(
        int id,
        CancellationToken ct = default);

    Task<Resultado<ProfessorResposta>> CriarAsync(
        ProfessorCriarRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<ProfessorResposta>> AtualizarAsync(
        int id,
        ProfessorAtualizarRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado> ExcluirAsync(
        int id,
        CancellationToken ct = default);
}

