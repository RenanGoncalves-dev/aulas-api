namespace Aulas.Api.Entidades;

public class Professor
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Materia { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal ValorHora { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }

    public ICollection<AulaMarcada> AulasMarcadas { get; private set; } = new List<AulaMarcada>();

    private Professor()
    {
    }

    public Professor(
        string nome,
        string materia,
        string descricao,
        decimal valorHora,
        int duracaoMinutos,
        bool ativo = true,
        DateTime? criadoEmUtc = null)
    {
        ValidarDados(nome, materia, descricao, valorHora, duracaoMinutos);

        var dataCriacao = criadoEmUtc ?? DateTime.UtcNow;
        if (dataCriacao.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de criação deve estar em UTC (DateTimeKind.Utc).", nameof(criadoEmUtc));

        Nome = nome.Trim();
        Materia = materia.Trim();
        Descricao = descricao.Trim();
        ValorHora = valorHora;
        DuracaoMinutos = duracaoMinutos;
        Ativo = ativo;
        CriadoEmUtc = dataCriacao;
    }

    public void Atualizar(
        string nome,
        string materia,
        string descricao,
        decimal valorHora,
        int duracaoMinutos,
        bool ativo)
    {
        ValidarDados(nome, materia, descricao, valorHora, duracaoMinutos);

        Nome = nome.Trim();
        Materia = materia.Trim();
        Descricao = descricao.Trim();
        ValorHora = valorHora;
        DuracaoMinutos = duracaoMinutos;
        Ativo = ativo;
    }

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    private static void ValidarDados(
        string nome,
        string materia,
        string descricao,
        decimal valorHora,
        int duracaoMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do professor é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(materia))
            throw new ArgumentException("A matéria é obrigatória.", nameof(materia));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));

        if (valorHora <= 0)
            throw new ArgumentException("O valor da hora deve ser maior que zero.", nameof(valorHora));

        if (duracaoMinutos < 15 || duracaoMinutos > 240)
            throw new ArgumentException("A duração da aula deve estar entre 15 e 240 minutos.", nameof(duracaoMinutos));
    }
}
