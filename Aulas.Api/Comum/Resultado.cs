namespace Aulas.Api.Comum;

public class Resultado
{
    public bool Sucesso { get; }
    public string? Erro { get; }
    public TipoErro? TipoErro { get; }

    protected Resultado(bool sucesso, string? erro, TipoErro? tipoErro)
    {
        Sucesso = sucesso;
        Erro = erro;
        TipoErro = tipoErro;
    }

    public static Resultado Ok() => new(true, null, null);

    public static Resultado Falha(string mensagem, TipoErro tipoErro) =>
        new(false, mensagem, tipoErro);
}

public sealed class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(bool sucesso, T? valor, string? erro, TipoErro? tipoErro)
        : base(sucesso, erro, tipoErro)
    {
        Valor = valor;
    }

    public static Resultado<T> Ok(T valor) =>
        new(true, valor, null, null);

    public new static Resultado<T> Falha(string mensagem, TipoErro tipoErro) =>
        new(false, default, mensagem, tipoErro);
}
