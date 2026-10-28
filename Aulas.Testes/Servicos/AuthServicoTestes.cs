using Aulas.Api.Comum;
using Aulas.Api.Dtos.Auth;
using Aulas.Api.Entidades;
using Aulas.Api.Servicos;
using Aulas.Testes.Infra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aulas.Testes.Servicos;

public class AuthServicoTestes
{
    private static IConfiguration CriarConfiguracaoJwt()
    {
        var valores = new Dictionary<string, string?>
        {
            ["Jwt:Segredo"] = "SegredoDeTesteMuitoSeguroComMaisDe32Bytes123456!",
            ["Jwt:Emissor"] = "Aulas.Api.Testes",
            ["Jwt:Publico"] = "Aulas.Clientes.Testes",
            ["Jwt:ExpiracaoHoras"] = "4"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();
    }

    [Fact]
    public async Task RegistrarAsync_DeveCriarClienteComSucessoERetornarToken()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new AuthServico(ctx.Db, CriarConfiguracaoJwt(), NullLogger<AuthServico>.Instance);

        var requisicao = new RegistroRequisicao(
            Nome: "Mariana Costa",
            Email: "Mariana@Exemplo.com",
            Senha: "SenhaForte123");

        var resultado = await servico.RegistrarAsync(requisicao);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("Mariana Costa", resultado.Valor!.Nome);
        Assert.Equal("mariana@exemplo.com", resultado.Valor.Email);
        Assert.Equal(Usuario.PapelCliente, resultado.Valor.Papel);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Valor.Token));
    }

    [Fact]
    public async Task RegistrarAsync_DeveRetornarConflito409QuandoEmailJaCadastrado()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new AuthServico(ctx.Db, CriarConfiguracaoJwt(), NullLogger<AuthServico>.Instance);

        var primeiro = new RegistroRequisicao("Carlos Silva", "carlos@exemplo.com", "Senha123");
        await servico.RegistrarAsync(primeiro);

        var duplicado = new RegistroRequisicao("Outro Carlos", "CARLOS@EXEMPLO.COM", "OutraSenha123");
        var resultado = await servico.RegistrarAsync(duplicado);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultado.TipoErro);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarTokenQuandoCredenciaisValidas()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new AuthServico(ctx.Db, CriarConfiguracaoJwt(), NullLogger<AuthServico>.Instance);

        await servico.RegistrarAsync(new RegistroRequisicao("Fernanda Lima", "fernanda@exemplo.com", "MinhaSenha123"));

        var resultado = await servico.LoginAsync(new LoginRequisicao("fernanda@exemplo.com", "MinhaSenha123"));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("fernanda@exemplo.com", resultado.Valor!.Email);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Valor.Token));
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarNaoAutorizado401QuandoSenhaIncorreta()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new AuthServico(ctx.Db, CriarConfiguracaoJwt(), NullLogger<AuthServico>.Instance);

        await servico.RegistrarAsync(new RegistroRequisicao("Fernanda Lima", "fernanda@exemplo.com", "MinhaSenha123"));

        var resultado = await servico.LoginAsync(new LoginRequisicao("fernanda@exemplo.com", "SenhaErrada999"));

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.NaoAutorizado, resultado.TipoErro);
    }
}

