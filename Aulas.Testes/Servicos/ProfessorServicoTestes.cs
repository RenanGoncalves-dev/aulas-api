using Aulas.Api.Dtos.Professores;
using Aulas.Api.Servicos;
using Aulas.Testes.Infra;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aulas.Testes.Servicos;

public class ProfessorServicoTestes
{
    private static async Task SemearProfessoresAsync(ProfessorServico servico)
    {
        await servico.CriarAsync(new ProfessorCriarRequisicao(
            Nome: "Clara Mendonça",
            Materia: "Violão e Guitarra",
            Descricao: "Aulas práticas de violão popular e guitarra.",
            ValorHora: 95.00m,
            DuracaoMinutos: 60,
            Ativo: true));

        await servico.CriarAsync(new ProfessorCriarRequisicao(
            Nome: "Rafael Vasconcelos",
            Materia: "Piano Clássico",
            Descricao: "Pianista com foco em técnica clássica e leitura de partitura.",
            ValorHora: 120.00m,
            DuracaoMinutos: 60,
            Ativo: true));

        await servico.CriarAsync(new ProfessorCriarRequisicao(
            Nome: "Lucas Alencar",
            Materia: "Inglês para Conversação",
            Descricao: "Aulas focadas em entrevistas e conversação fluente.",
            ValorHora: 105.00m,
            DuracaoMinutos: 60,
            Ativo: true));
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListagemPaginadaCorretamente()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new ProfessorServico(ctx.Db, NullLogger<ProfessorServico>.Instance);
        await SemearProfessoresAsync(servico);

        var resultado = await servico.ListarAsync(busca: null, pagina: 1, tamanho: 2);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(2, resultado.Valor!.Itens.Count);
        Assert.Equal(3, resultado.Valor.Total);
        Assert.Equal(2, resultado.Valor.TotalPaginas);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorTermoExistenteNoNomeOuMateria()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new ProfessorServico(ctx.Db, NullLogger<ProfessorServico>.Instance);
        await SemearProfessoresAsync(servico);

        var resultado = await servico.ListarAsync(busca: "VIOLÃO", pagina: 1, tamanho: 10);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Single(resultado.Valor!.Itens);
        Assert.Equal("Clara Mendonça", resultado.Valor.Itens[0].Nome);
    }

    [Fact]
    public async Task ListarAsync_DeveRetornarListaVaziaETotalZeroQuandoTermoInexistente()
    {
        await using var ctx = new ContextoSqliteTeste();
        var servico = new ProfessorServico(ctx.Db, NullLogger<ProfessorServico>.Instance);
        await SemearProfessoresAsync(servico);

        var resultado = await servico.ListarAsync(busca: "mandarim-avancado", pagina: 1, tamanho: 10);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Empty(resultado.Valor!.Itens);
        Assert.Equal(0, resultado.Valor.Total);
        Assert.Equal(0, resultado.Valor.TotalPaginas);
    }
}

