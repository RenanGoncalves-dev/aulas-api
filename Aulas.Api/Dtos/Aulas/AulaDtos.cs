using System.ComponentModel.DataAnnotations;

namespace Aulas.Api.Dtos.Aulas;

public record CriarAulaDto(
    [Range(1, int.MaxValue, ErrorMessage = "O identificador do professor deve ser válido.")]
    int ProfessorId,

    [Required(ErrorMessage = "A data/hora de início é obrigatória.")]
    DateTimeOffset DataHoraInicio,

    [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres.")]
    string? Observacoes = null);

public record AulaResposta(
    int Id,
    int ProfessorId,
    string ProfessorNome,
    string ProfessorMateria,
    int UsuarioId,
    string UsuarioNome,
    DateTime DataHoraInicioUtc,
    DateTime DataHoraFimUtc,
    string Observacoes,
    string Status,
    DateTime CriadoEmUtc);
