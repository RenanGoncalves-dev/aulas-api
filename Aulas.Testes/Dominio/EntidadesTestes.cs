using Aulas.Api.Comum;
using Aulas.Api.Entidades;

namespace Aulas.Testes.Dominio;

public class EntidadesTestes
{
    [Fact]
    public void AulaMarcada_DeveRejeitarDatasComKindDiferenteDeUtc()
    {
        var inicioLocal = new DateTime(2026, 11, 10, 14, 0, 0, DateTimeKind.Local);
        var fimUtc = new DateTime(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);

        var excecao = Assert.Throws<ArgumentException>(() =>
            new AulaMarcada(
                professorId: 1,
                usuarioId: 1,
                dataHoraInicioUtc: inicioLocal,
                dataHoraFimUtc: fimUtc,
                observacoes: "Teste Kind Local"));

        Assert.Contains("UTC", excecao.Message);
    }

    [Fact]
    public void AulaMarcada_DeveRejeitarDataFimAnteriorOuIgualADataInicio()
    {
        var inicioUtc = new DateTime(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);
        var fimUtc = new DateTime(2026, 11, 10, 14, 0, 0, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() =>
            new AulaMarcada(
                professorId: 1,
                usuarioId: 1,
                dataHoraInicioUtc: inicioUtc,
                dataHoraFimUtc: fimUtc,
                observacoes: "Horário invertido"));
    }

    [Fact]
    public void AulaMarcada_Cancelar_DeveAlterarStatusParaCancelada()
    {
        var inicioUtc = new DateTime(2026, 11, 10, 14, 0, 0, DateTimeKind.Utc);
        var fimUtc = new DateTime(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);

        var aula = new AulaMarcada(1, 2, inicioUtc, fimUtc, "Aula experimental");
        Assert.Equal(StatusAula.Agendada, aula.Status);

        aula.Cancelar();

        Assert.Equal(StatusAula.Cancelada, aula.Status);
    }

    [Fact]
    public void Professor_DeveCriarComSucessoQuandoDadosValidosEmUtc()
    {
        var criadoEm = new DateTime(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);

        var professor = new Professor(
            nome: "Clara Mendonça",
            materia: "Violão e Guitarra",
            descricao: "Aulas práticas de violão popular e harmonia.",
            valorHora: 95.00m,
            duracaoMinutos: 60,
            ativo: true,
            criadoEmUtc: criadoEm);

        Assert.Equal("Clara Mendonça", professor.Nome);
        Assert.Equal("Violão e Guitarra", professor.Materia);
        Assert.Equal(95.00m, professor.ValorHora);
        Assert.Equal(60, professor.DuracaoMinutos);
        Assert.True(professor.Ativo);
        Assert.Equal(DateTimeKind.Utc, professor.CriadoEmUtc.Kind);
    }

    [Fact]
    public void Usuario_DeveNormalizarEmailERejeitarPapelInvalido()
    {
        var usuario = new Usuario(
            nome: "  Renan Gonçalves ",
            email: "  RENAN@Exemplo.com ",
            senhaHash: "hash-seguro",
            papel: "CLIENTE");

        Assert.Equal("Renan Gonçalves", usuario.Nome);
        Assert.Equal("renan@exemplo.com", usuario.Email);
        Assert.Equal(Usuario.PapelCliente, usuario.Papel);

        Assert.Throws<ArgumentException>(() =>
            new Usuario("Teste", "teste@exemplo.com", "hash", "moderador"));
    }

    [Fact]
    public void Resultado_DeveRepresentarSucessoEFalhaComTipoErro()
    {
        var sucesso = Resultado<int>.Ok(42);
        var falha = Resultado<int>.Falha("Conflito de horário", TipoErro.Conflito);

        Assert.True(sucesso.Sucesso);
        Assert.Equal(42, sucesso.Valor);
        Assert.Null(sucesso.TipoErro);

        Assert.False(falha.Sucesso);
        Assert.Equal("Conflito de horário", falha.Erro);
        Assert.Equal(TipoErro.Conflito, falha.TipoErro);
    }
}
