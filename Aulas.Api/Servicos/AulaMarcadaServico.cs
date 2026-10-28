using Aulas.Api.Comum;
using Aulas.Api.Dados;
using Aulas.Api.Dtos.Aulas;
using Aulas.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Aulas.Api.Servicos;

public class AulaMarcadaServico : IAulaMarcadaServico
{
    private readonly AppDbContext _db;
    private readonly ILogger<AulaMarcadaServico> _logger;

    public AulaMarcadaServico(AppDbContext db, ILogger<AulaMarcadaServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Resultado<AulaResposta>> CriarAsync(
        int usuarioId,
        CriarAulaDto dto,
        CancellationToken ct = default)
    {
        if (usuarioId <= 0)
        {
            return Resultado<AulaResposta>.Falha(
                "Usuário não autenticado ou identificador inválido.",
                TipoErro.NaoAutorizado);
        }

        if (dto.ProfessorId <= 0)
        {
            return Resultado<AulaResposta>.Falha(
                "O identificador do professor deve ser válido.",
                TipoErro.Validacao);
        }

        if (dto.Observacoes is not null && dto.Observacoes.Length > 500)
        {
            return Resultado<AulaResposta>.Falha(
                "As observações devem ter no máximo 500 caracteres.",
                TipoErro.Validacao);
        }

        var inicioNovoUtc = DateTime.SpecifyKind(dto.DataHoraInicio.UtcDateTime, DateTimeKind.Utc);

        await using var transacao = await _db.Database.BeginTransactionAsync(ct);

        var professor = await _db.Professores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.ProfessorId, ct);

        if (professor is null)
        {
            return Resultado<AulaResposta>.Falha(
                "Professor não encontrado.",
                TipoErro.NaoEncontrado);
        }

        if (!professor.Ativo)
        {
            return Resultado<AulaResposta>.Falha(
                "Este professor está com a agenda inativa e não aceita novos agendamentos no momento.",
                TipoErro.Conflito);
        }

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, ct);

        if (usuario is null)
        {
            return Resultado<AulaResposta>.Falha(
                "Usuário autenticado não encontrado.",
                TipoErro.NaoEncontrado);
        }

        var fimNovoUtc = DateTime.SpecifyKind(
            inicioNovoUtc.AddMinutes(professor.DuracaoMinutos),
            DateTimeKind.Utc);

        var existeConflito = await _db.AulasMarcadas
            .AnyAsync(a =>
                a.ProfessorId == professor.Id &&
                a.Status == StatusAula.Agendada &&
                a.DataHoraInicioUtc < fimNovoUtc &&
                a.DataHoraFimUtc > inicioNovoUtc,
                ct);

        if (existeConflito)
        {
            _logger.LogWarning(
                "Conflito de horário ao agendar aula: ProfessorId={ProfessorId}, UsuarioId={UsuarioId}, InicioUtc={InicioUtc}, FimUtc={FimUtc}",
                professor.Id,
                usuarioId,
                inicioNovoUtc,
                fimNovoUtc);

            return Resultado<AulaResposta>.Falha(
                "O professor selecionado já possui uma aula agendada que conflita com este horário.",
                TipoErro.Conflito);
        }

        var aula = new AulaMarcada(
            professorId: professor.Id,
            usuarioId: usuarioId,
            dataHoraInicioUtc: inicioNovoUtc,
            dataHoraFimUtc: fimNovoUtc,
            observacoes: dto.Observacoes,
            criadoEmUtc: DateTime.UtcNow);

        _db.AulasMarcadas.Add(aula);
        await _db.SaveChangesAsync(ct);
        await transacao.CommitAsync(ct);

        _logger.LogInformation(
            "Aula agendada com sucesso: AulaId={AulaId}, ProfessorId={ProfessorId}, UsuarioId={UsuarioId}, InicioUtc={InicioUtc}",
            aula.Id,
            professor.Id,
            usuarioId,
            aula.DataHoraInicioUtc);

        var resposta = new AulaResposta(
            Id: aula.Id,
            ProfessorId: professor.Id,
            ProfessorNome: professor.Nome,
            ProfessorMateria: professor.Materia,
            UsuarioId: usuario.Id,
            UsuarioNome: usuario.Nome,
            DataHoraInicioUtc: aula.DataHoraInicioUtc,
            DataHoraFimUtc: aula.DataHoraFimUtc,
            Observacoes: aula.Observacoes,
            Status: aula.Status.ToString(),
            CriadoEmUtc: aula.CriadoEmUtc);

        return Resultado<AulaResposta>.Ok(resposta);
    }

    public async Task<Resultado<IReadOnlyList<AulaResposta>>> ListarAsync(
        int usuarioId,
        string papel,
        CancellationToken ct = default)
    {
        var query = _db.AulasMarcadas
            .AsNoTracking()
            .Include(a => a.Professor)
            .Include(a => a.Usuario)
            .AsQueryable();

        var ehAdmin = string.Equals(papel, Usuario.PapelAdmin, StringComparison.OrdinalIgnoreCase);

        if (!ehAdmin)
        {
            query = query.Where(a => a.UsuarioId == usuarioId);
        }

        var aulas = await query
            .OrderBy(a => a.DataHoraInicioUtc)
            .ThenBy(a => a.Id)
            .Select(a => new AulaResposta(
                a.Id,
                a.ProfessorId,
                a.Professor != null ? a.Professor.Nome : string.Empty,
                a.Professor != null ? a.Professor.Materia : string.Empty,
                a.UsuarioId,
                a.Usuario != null ? a.Usuario.Nome : string.Empty,
                a.DataHoraInicioUtc,
                a.DataHoraFimUtc,
                a.Observacoes,
                a.Status.ToString(),
                a.CriadoEmUtc))
            .ToListAsync(ct);

        return Resultado<IReadOnlyList<AulaResposta>>.Ok(aulas);
    }

    public async Task<Resultado> CancelarAsync(
        int aulaId,
        int usuarioId,
        string papel,
        CancellationToken ct = default)
    {
        var aula = await _db.AulasMarcadas
            .FirstOrDefaultAsync(a => a.Id == aulaId, ct);

        if (aula is null)
        {
            return Resultado.Falha(
                "Aula marcada não encontrada.",
                TipoErro.NaoEncontrado);
        }

        var ehAdmin = string.Equals(papel, Usuario.PapelAdmin, StringComparison.OrdinalIgnoreCase);

        if (aula.UsuarioId != usuarioId && !ehAdmin)
        {
            _logger.LogWarning(
                "Tentativa não autorizada (IDOR) de cancelar aula: AulaId={AulaId}, DonoId={DonoId}, SolicitanteId={SolicitanteId}",
                aula.Id,
                aula.UsuarioId,
                usuarioId);

            return Resultado.Falha(
                "Você não tem permissão para cancelar uma aula que pertence a outro usuário.",
                TipoErro.Proibido);
        }

        if (aula.Status == StatusAula.Cancelada)
        {
            return Resultado.Falha(
                "Esta aula já se encontra cancelada.",
                TipoErro.Conflito);
        }

        aula.Cancelar();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Aula cancelada com sucesso: AulaId={AulaId}, SolicitanteId={SolicitanteId}, Papel={Papel}",
            aula.Id,
            usuarioId,
            papel);

        return Resultado.Ok();
    }
}

