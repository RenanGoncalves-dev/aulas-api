using System.ComponentModel.DataAnnotations;

namespace Aulas.Api.Dtos.Auth;

public record RegistroRequisicao(
    [property: Required(ErrorMessage = "O nome é obrigatório.")]
    [property: StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    string Nome,

    [property: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [property: EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    [property: StringLength(160, ErrorMessage = "O e-mail deve ter no máximo 160 caracteres.")]
    string Email,

    [property: Required(ErrorMessage = "A senha é obrigatória.")]
    [property: StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    string Senha);

public record LoginRequisicao(
    [property: Required(ErrorMessage = "O e-mail é obrigatório.")]
    [property: EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
    string Email,

    [property: Required(ErrorMessage = "A senha é obrigatória.")]
    string Senha);

public record AuthResposta(
    int UsuarioId,
    string Nome,
    string Email,
    string Papel,
    string Token,
    DateTime ExpiraEmUtc);
