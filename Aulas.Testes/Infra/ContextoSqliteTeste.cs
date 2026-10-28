using Aulas.Api.Dados;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Aulas.Testes.Infra;

public sealed class ContextoSqliteTeste : IAsyncDisposable, IDisposable
{
    private readonly SqliteConnection _conexao;
    public AppDbContext Db { get; }

    public ContextoSqliteTeste()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexao)
            .Options;

        Db = new AppDbContext(opcoes);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _conexao.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _conexao.DisposeAsync();
    }
}

