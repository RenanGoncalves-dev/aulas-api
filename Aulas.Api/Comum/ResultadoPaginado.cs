namespace Aulas.Api.Comum;

public record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int Tamanho,
    int Total,
    int TotalPaginas);
