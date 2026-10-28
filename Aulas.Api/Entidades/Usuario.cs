namespace Aulas.Api.Entidades;

public class Usuario
{
    public const string PapelAdmin = "admin";
    public const string PapelCliente = "cliente";

    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string Papel { get; private set; } = PapelCliente;
    public DateTime CriadoEmUtc { get; private set; }

    private Usuario()
    {
    }

    public Usuario(
        string nome,
        string email,
        string senhaHash,
        string papel = PapelCliente,
        DateTime? criadoEmUtc = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuário é obrigatório.", nameof(nome));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("O e-mail do usuário é inválido.", nameof(email));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(senhaHash));

        var papelNormalizado = papel.Trim().ToLowerInvariant();
        if (papelNormalizado != PapelAdmin && papelNormalizado != PapelCliente)
            throw new ArgumentException("O papel deve ser 'admin' ou 'cliente'.", nameof(papel));

        var dataCriacao = criadoEmUtc ?? DateTime.UtcNow;
        if (dataCriacao.Kind != DateTimeKind.Utc)
            throw new ArgumentException("A data de criação deve estar em UTC (DateTimeKind.Utc).", nameof(criadoEmUtc));

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        Papel = papelNormalizado;
        CriadoEmUtc = dataCriacao;
    }

    public void AtualizarSenhaHash(string novoSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novoSenhaHash))
            throw new ArgumentException("O hash da senha é obrigatório.", nameof(novoSenhaHash));

        SenhaHash = novoSenhaHash;
    }
}
