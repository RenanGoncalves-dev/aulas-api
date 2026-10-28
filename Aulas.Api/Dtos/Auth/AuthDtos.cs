using System.ComponentModel.DataAnnotations;

namespace Aulas.Api.Dtos.Auth;

public record RegistroRequisicao(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [StringLength(160, ErrorMessage = "O e-mail deve ter no máximo 160 caracteres.")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    string Senha);

public record LoginRequisicao(
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    string Email,

    [Required(ErrorMessage = "A senha é obrigatória.")]
    string Senha);

public record AuthResposta(
    int UsuarioId,
    string Nome,
    string Email,
    string Papel,
    string Token,
    DateTime ExpiraEmUtc);
