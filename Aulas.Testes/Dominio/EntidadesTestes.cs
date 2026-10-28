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
}
