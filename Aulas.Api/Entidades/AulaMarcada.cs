namespace Aulas.Api.Entidades;

public class AulaMarcada
{
    public int Id { get; private set; }
    public int ProfessorId { get; private set; }
    public Professor? Professor { get; private set; }
    public int UsuarioId { get; private set; }
    public Usuario? Usuario { get; private set; }
    public DateTime DataHoraInicioUtc { get; private set; }
    public DateTime DataHoraFimUtc { get; private set; }
    public string Observacoes { get; private set; } = string.Empty;
    public StatusAula Status { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }

    private AulaMarcada()
    {
    }

    public AulaMarcada(
        int professorId,
        int usuarioId,
        DateTime dataHoraInicioUtc,
        DateTime dataHoraFimUtc,
        string? observacoes = null,
        DateTime? criadoEmUtc = null)
    {
        if (professorId <= 0)
            throw new ArgumentException("O identificador do professor deve ser válido.", nameof(professorId));

        if (usuarioId <= 0)
            throw new ArgumentException("O identificador do usuário deve ser válido.", nameof(usuarioId));

        if (dataHoraInicioUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data/hora de início deve estar em UTC (DateTimeKind.Utc).", nameof(dataHoraInicioUtc));

        if (dataHoraFimUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data/hora de fim deve estar em UTC (DateTimeKind.Utc).", nameof(dataHoraFimUtc));

        if (dataHoraFimUtc <= dataHoraInicioUtc)
            throw new ArgumentException("A data/hora de fim deve ser posterior à data/hora de início.", nameof(dataHoraFimUtc));

        var dataCriacao = criadoEmUtc ?? DateTime.UtcNow;
        if (dataCriacao.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de criação deve estar em UTC (DateTimeKind.Utc).", nameof(criadoEmUtc));

        ProfessorId = professorId;
        UsuarioId = usuarioId;
        DataHoraInicioUtc = dataHoraInicioUtc;
        DataHoraFimUtc = dataHoraFimUtc;
        Observacoes = observacoes?.Trim() ?? string.Empty;
        Status = StatusAula.Agendada;
        CriadoEmUtc = dataCriacao;
    }

    public void Cancelar()
    {
        Status = StatusAula.Cancelada;
    }
}
