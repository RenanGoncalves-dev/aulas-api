using Aulas.Api.Comum;
using Aulas.Api.Dados;
using Aulas.Api.Dtos.Aulas;
using Aulas.Api.Entidades;
using Aulas.Api.Servicos;
using Aulas.Testes.Infra;
using Microsoft.Extensions.Logging.Abstractions;

namespace Aulas.Testes.Servicos;

public class AulaMarcadaServicoTestes
{
    private static async Task<(Professor professor, Usuario clienteA, Usuario clienteB)> SemearCenarioBaseAsync(AppDbContext db)
    {
        var professor = new Professor(
            nome: "Clara Mendonça",
            materia: "Violão e Guitarra",
            descricao: "Aulas práticas de violão.",
            valorHora: 95.00m,
            duracaoMinutos: 60,
            ativo: true,
            criadoEmUtc: DateTime.UtcNow);

        var clienteA = new Usuario(
            nome: "Ana Cliente A",
            email: "ana@exemplo.com",
            senhaHash: "hash-a",
            papel: Usuario.PapelCliente,
            criadoEmUtc: DateTime.UtcNow);

        var clienteB = new Usuario(
            nome: "Bruno Cliente B",
            email: "bruno@exemplo.com",
            senhaHash: "hash-b",
            papel: Usuario.PapelCliente,
            criadoEmUtc: DateTime.UtcNow);

        db.Professores.Add(professor);
        db.Usuarios.AddRange(clienteA, clienteB);
        await db.SaveChangesAsync();

        return (professor, clienteA, clienteB);
    }

    [Fact]
    public async Task CriarAsync_DeveAgendarAulaComSucessoEmUtc()
    {
        await using var ctx = new ContextoSqliteTeste();
        var (professor, clienteA, _) = await SemearCenarioBaseAsync(ctx.Db);
        var servico = new AulaMarcadaServico(ctx.Db, NullLogger<AulaMarcadaServico>.Instance);

        var inicioOffset = new DateTimeOffset(2026, 11, 15, 14, 0, 0, TimeSpan.FromHours(-3));
        var dto = new CriarAulaDto(
            ProfessorId: professor.Id,
            DataHoraInicio: inicioOffset,
            Observacoes: "Primeira aula experimental.");

        var resultado = await servico.CriarAsync(clienteA.Id, dto);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal(professor.Id, resultado.Valor!.ProfessorId);
        Assert.Equal(clienteA.Id, resultado.Valor.UsuarioId);
        Assert.Equal(new DateTime(2026, 11, 15, 17, 0, 0, DateTimeKind.Utc), resultado.Valor.DataHoraInicioUtc);
        Assert.Equal(new DateTime(2026, 11, 15, 18, 0, 0, DateTimeKind.Utc), resultado.Valor.DataHoraFimUtc);
    }

    [Fact]
    public async Task CriarAsync_DeveRetornarConflito409QuandoHorarioSobrepostoParaMesmoProfessor()
    {
        await using var ctx = new ContextoSqliteTeste();
        var (professor, clienteA, clienteB) = await SemearCenarioBaseAsync(ctx.Db);
        var servico = new AulaMarcadaServico(ctx.Db, NullLogger<AulaMarcadaServico>.Instance);

        // Aula 1: das 14:00 às 15:00 UTC
        var inicio1 = new DateTimeOffset(2026, 11, 20, 14, 0, 0, TimeSpan.Zero);
        var primeira = await servico.CriarAsync(clienteA.Id, new CriarAulaDto(professor.Id, inicio1, "Aula 1"));
        Assert.True(primeira.Sucesso);

        // Aula 2 tentando iniciar às 14:30 UTC (sobrepõe o intervalo 14:00-15:00)
        var inicioSobreposto = new DateTimeOffset(2026, 11, 20, 14, 30, 0, TimeSpan.Zero);
        var conflito = await servico.CriarAsync(clienteB.Id, new CriarAulaDto(professor.Id, inicioSobreposto, "Aula sobreposta"));

        Assert.False(conflito.Sucesso);
        Assert.Equal(TipoErro.Conflito, conflito.TipoErro);
    }

    [Fact]
    public async Task CancelarAsync_DeveRetornarProibido403QuandoClienteBTentaCancelarAulaDoClienteA()
    {
        await using var ctx = new ContextoSqliteTeste();
        var (professor, clienteA, clienteB) = await SemearCenarioBaseAsync(ctx.Db);
        var servico = new AulaMarcadaServico(ctx.Db, NullLogger<AulaMarcadaServico>.Instance);

        var inicio = new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.Zero);
        var criada = await servico.CriarAsync(clienteA.Id, new CriarAulaDto(professor.Id, inicio, "Aula da Ana"));
        Assert.True(criada.Sucesso);

        // Cliente B tenta cancelar a aula do Cliente A (IDOR)
        var resultado = await servico.CancelarAsync(
            aulaId: criada.Valor!.Id,
            usuarioId: clienteB.Id,
            papel: Usuario.PapelCliente);

        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.Proibido, resultado.TipoErro);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarApenasAulasDoProprioCliente()
    {
        await using var ctx = new ContextoSqliteTeste();
        var (professor, clienteA, clienteB) = await SemearCenarioBaseAsync(ctx.Db);
        var servico = new AulaMarcadaServico(ctx.Db, NullLogger<AulaMarcadaServico>.Instance);

        await servico.CriarAsync(
            clienteA.Id,
            new CriarAulaDto(professor.Id, new DateTimeOffset(2026, 11, 22, 9, 0, 0, TimeSpan.Zero), "Aula Cliente A"));

        await servico.CriarAsync(
            clienteB.Id,
            new CriarAulaDto(professor.Id, new DateTimeOffset(2026, 11, 22, 11, 0, 0, TimeSpan.Zero), "Aula Cliente B"));

        var listaClienteA = await servico.ListarAsync(clienteA.Id, Usuario.PapelCliente);

        Assert.True(listaClienteA.Sucesso);
        Assert.NotNull(listaClienteA.Valor);
        Assert.Single(listaClienteA.Valor!);
        Assert.Equal(clienteA.Id, listaClienteA.Valor[0].UsuarioId);
    }
}

