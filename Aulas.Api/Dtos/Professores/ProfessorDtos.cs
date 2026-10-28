using System.ComponentModel.DataAnnotations;

namespace Aulas.Api.Dtos.Professores;

public record ProfessorCriarRequisicao(
    [property: Required(ErrorMessage = "O nome do professor é obrigatório.")]
    [property: StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    string Nome,

    [property: Required(ErrorMessage = "A matéria é obrigatória.")]
    [property: StringLength(80, MinimumLength = 2, ErrorMessage = "A matéria deve ter entre 2 e 80 caracteres.")]
    string Materia,

    [property: Required(ErrorMessage = "A descrição é obrigatória.")]
    [property: StringLength(600, MinimumLength = 5, ErrorMessage = "A descrição deve ter entre 5 e 600 caracteres.")]
    string Descricao,

    [property: Range(0.01, 10000.00, ErrorMessage = "O valor da hora deve ser maior que zero.")]
    decimal ValorHora,

    [property: Range(15, 240, ErrorMessage = "A duração da aula deve estar entre 15 e 240 minutos.")]
    int DuracaoMinutos,

    bool Ativo = true);

public record ProfessorAtualizarRequisicao(
    [property: Required(ErrorMessage = "O nome do professor é obrigatório.")]
    [property: StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    string Nome,

    [property: Required(ErrorMessage = "A matéria é obrigatória.")]
    [property: StringLength(80, MinimumLength = 2, ErrorMessage = "A matéria deve ter entre 2 e 80 caracteres.")]
    string Materia,

    [property: Required(ErrorMessage = "A descrição é obrigatória.")]
    [property: StringLength(600, MinimumLength = 5, ErrorMessage = "A descrição deve ter entre 5 e 600 caracteres.")]
    string Descricao,

    [property: Range(0.01, 10000.00, ErrorMessage = "O valor da hora deve ser maior que zero.")]
    decimal ValorHora,

    [property: Range(15, 240, ErrorMessage = "A duração da aula deve estar entre 15 e 240 minutos.")]
    int DuracaoMinutos,

    bool Ativo = true);

public record ProfessorResposta(
    int Id,
    string Nome,
    string Materia,
    string Descricao,
    decimal ValorHora,
    int DuracaoMinutos,
    bool Ativo,
    DateTime CriadoEmUtc);

