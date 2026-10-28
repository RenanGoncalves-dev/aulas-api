using Aulas.Api.Comum;
using Aulas.Api.Dados;
using Aulas.Api.Dtos.Professores;
using Aulas.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Aulas.Api.Servicos;

public class ProfessorServico : IProfessorServico
{
    private readonly AppDbContext _db;
    private readonly ILogger<ProfessorServico> _logger;

    public ProfessorServico(AppDbContext db, ILogger<ProfessorServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Resultado<ResultadoPaginado<ProfessorResposta>>> ListarAsync(
        string? busca = null,
        int pagina = 1,
        int tamanho = 10,
        CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1) tamanho = 10;
        if (tamanho > 100) tamanho = 100;

        var query = _db.Professores.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Materia.ToLower().Contains(termo));
        }

        var total = await query.CountAsync(ct);

        if (total == 0)
        {
            var vazio = new ResultadoPaginado<ProfessorResposta>(
                Itens: Array.Empty<ProfessorResposta>(),
                Pagina: pagina,
                Tamanho: tamanho,
                Total: 0,
                TotalPaginas: 0);

            return Resultado<ResultadoPaginado<ProfessorResposta>>.Ok(vazio);
        }

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);

        var itens = await query
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(p => MapearParaResposta(p))
            .ToListAsync(ct);

        var resultadoPaginado = new ResultadoPaginado<ProfessorResposta>(
            Itens: itens,
            Pagina: pagina,
            Tamanho: tamanho,
            Total: total,
            TotalPaginas: totalPaginas);

        return Resultado<ResultadoPaginado<ProfessorResposta>>.Ok(resultadoPaginado);
    }

    public async Task<Resultado<ProfessorResposta>> ObterPorIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var professor = await _db.Professores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (professor is null)
        {
            return Resultado<ProfessorResposta>.Falha(
                "Professor não encontrado.",
                TipoErro.NaoEncontrado);
        }

        return Resultado<ProfessorResposta>.Ok(MapearParaResposta(professor));
    }

    public async Task<Resultado<ProfessorResposta>> CriarAsync(
        ProfessorCriarRequisicao requisicao,
        CancellationToken ct = default)
    {
        var erroValidacao = ValidarDadosProfessor(
            requisicao.Nome,
            requisicao.Materia,
            requisicao.Descricao,
            requisicao.ValorHora,
            requisicao.DuracaoMinutos);

        if (erroValidacao is not null)
        {
            return Resultado<ProfessorResposta>.Falha(erroValidacao, TipoErro.Validacao);
        }

        var professor = new Professor(
            nome: requisicao.Nome,
            materia: requisicao.Materia,
            descricao: requisicao.Descricao,
            valorHora: requisicao.ValorHora,
            duracaoMinutos: requisicao.DuracaoMinutos,
            ativo: requisicao.Ativo,
            criadoEmUtc: DateTime.UtcNow);

        _db.Professores.Add(professor);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Professor criado com sucesso: ProfessorId={ProfessorId}, Nome={Nome}, Materia={Materia}",
            professor.Id,
            professor.Nome,
            professor.Materia);

        return Resultado<ProfessorResposta>.Ok(MapearParaResposta(professor));
    }

    public async Task<Resultado<ProfessorResposta>> AtualizarAsync(
        int id,
        ProfessorAtualizarRequisicao requisicao,
        CancellationToken ct = default)
    {
        var erroValidacao = ValidarDadosProfessor(
            requisicao.Nome,
            requisicao.Materia,
            requisicao.Descricao,
            requisicao.ValorHora,
            requisicao.DuracaoMinutos);

        if (erroValidacao is not null)
        {
            return Resultado<ProfessorResposta>.Falha(erroValidacao, TipoErro.Validacao);
        }

        var professor = await _db.Professores
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (professor is null)
        {
            return Resultado<ProfessorResposta>.Falha(
                "Professor não encontrado.",
                TipoErro.NaoEncontrado);
        }

        professor.Atualizar(
            nome: requisicao.Nome,
            materia: requisicao.Materia,
            descricao: requisicao.Descricao,
            valorHora: requisicao.ValorHora,
            duracaoMinutos: requisicao.DuracaoMinutos,
            ativo: requisicao.Ativo);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Professor atualizado com sucesso: ProfessorId={ProfessorId}, Nome={Nome}, Ativo={Ativo}",
            professor.Id,
            professor.Nome,
            professor.Ativo);

        return Resultado<ProfessorResposta>.Ok(MapearParaResposta(professor));
    }

    public async Task<Resultado> ExcluirAsync(
        int id,
        CancellationToken ct = default)
    {
        var professor = await _db.Professores
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (professor is null)
        {
            return Resultado.Falha(
                "Professor não encontrado.",
                TipoErro.NaoEncontrado);
        }

        var possuiAulasVinculadas = await _db.AulasMarcadas
            .AnyAsync(a => a.ProfessorId == id, ct);

        if (possuiAulasVinculadas)
        {
            _logger.LogWarning(
                "Tentativa de exclusão de professor com aulas vinculadas bloqueada: ProfessorId={ProfessorId}, Nome={Nome}",
                professor.Id,
                professor.Nome);

            return Resultado.Falha(
                "Não é possível excluir o professor porque existem aulas marcadas vinculadas a ele. Desative o cadastro se desejar pausar novos agendamentos.",
                TipoErro.Conflito);
        }

        _db.Professores.Remove(professor);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Professor excluído com sucesso: ProfessorId={ProfessorId}, Nome={Nome}",
            professor.Id,
            professor.Nome);

        return Resultado.Ok();
    }

    private static string? ValidarDadosProfessor(
        string? nome,
        string? materia,
        string? descricao,
        decimal valorHora,
        int duracaoMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return "O nome do professor é obrigatório.";

        if (string.IsNullOrWhiteSpace(materia))
            return "A matéria é obrigatória.";

        if (string.IsNullOrWhiteSpace(descricao))
            return "A descrição é obrigatória.";

        if (valorHora <= 0)
            return "O valor da hora deve ser maior que zero.";

        if (duracaoMinutos < 15 || duracaoMinutos > 240)
            return "A duração da aula deve estar entre 15 e 240 minutos.";

        return null;
    }

    private static ProfessorResposta MapearParaResposta(Professor p) =>
        new(
            Id: p.Id,
            Nome: p.Nome,
            Materia: p.Materia,
            Descricao: p.Descricao,
            ValorHora: p.ValorHora,
            DuracaoMinutos: p.DuracaoMinutos,
            Ativo: p.Ativo,
            CriadoEmUtc: p.CriadoEmUtc);
}

