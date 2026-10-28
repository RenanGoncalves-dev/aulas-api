using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aulas.Api.Comum;
using Aulas.Api.Dados;
using Aulas.Api.Dtos.Auth;
using Aulas.Api.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Aulas.Api.Servicos;

public class AuthServico : IAuthServico
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthServico> _logger;

    public AuthServico(
        AppDbContext db,
        IConfiguration configuration,
        ILogger<AuthServico> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Resultado<AuthResposta>> RegistrarAsync(
        RegistroRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Nome) ||
            string.IsNullOrWhiteSpace(requisicao.Email) ||
            string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return Resultado<AuthResposta>.Falha(
                "Nome, e-mail e senha são obrigatórios.",
                TipoErro.Validacao);
        }

        if (requisicao.Senha.Length < 6)
        {
            return Resultado<AuthResposta>.Falha(
                "A senha deve ter no mínimo 6 caracteres.",
                TipoErro.Validacao);
        }

        var emailNormalizado = requisicao.Email.Trim().ToLowerInvariant();

        var emailJaExiste = await _db.Usuarios
            .AnyAsync(u => u.Email == emailNormalizado, ct);

        if (emailJaExiste)
        {
            _logger.LogWarning("Tentativa de registro com e-mail já cadastrado: {Email}", emailNormalizado);
            return Resultado<AuthResposta>.Falha(
                "Já existe um usuário cadastrado com este endereço de e-mail.",
                TipoErro.Conflito);
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(requisicao.Senha);

        // O registro público cria sempre cliente; admin só nasce no seed
        var usuario = new Usuario(
            nome: requisicao.Nome,
            email: emailNormalizado,
            senhaHash: senhaHash,
            papel: Usuario.PapelCliente,
            criadoEmUtc: DateTime.UtcNow);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Novo cliente registrado com sucesso: UsuarioId={UsuarioId}, Email={Email}",
            usuario.Id,
            usuario.Email);

        var resposta = GerarTokenResposta(usuario);
        return Resultado<AuthResposta>.Ok(resposta);
    }

    public async Task<Resultado<AuthResposta>> LoginAsync(
        LoginRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Email) ||
            string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return Resultado<AuthResposta>.Falha(
                "E-mail e senha são obrigatórios.",
                TipoErro.Validacao);
        }

        var emailNormalizado = requisicao.Email.Trim().ToLowerInvariant();

        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == emailNormalizado, ct);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(requisicao.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Falha de login para o e-mail {Email}", emailNormalizado);
            return Resultado<AuthResposta>.Falha(
                "E-mail ou senha inválidos.",
                TipoErro.NaoAutorizado);
        }

        _logger.LogInformation(
            "Login realizado com sucesso: UsuarioId={UsuarioId}, Papel={Papel}",
            usuario.Id,
            usuario.Papel);

        var resposta = GerarTokenResposta(usuario);
        return Resultado<AuthResposta>.Ok(resposta);
    }

    private AuthResposta GerarTokenResposta(Usuario usuario)
    {
        var segredo = _configuration["Jwt:Segredo"];
        if (string.IsNullOrWhiteSpace(segredo) || Encoding.UTF8.GetByteCount(segredo) < 32)
        {
            throw new InvalidOperationException(
                "O segredo JWT ('Jwt:Segredo') não está configurado corretamente.");
        }

        var emissor = _configuration["Jwt:Emissor"] ?? "Aulas.Api";
        var publico = _configuration["Jwt:Publico"] ?? "Aulas.Clientes";
        var expiracaoHoras = int.TryParse(_configuration["Jwt:ExpiracaoHoras"], out var horas) && horas > 0
            ? horas
            : 8;

        var expiraEmUtc = DateTime.UtcNow.AddHours(expiracaoHoras);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Papel)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredo));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: emissor,
            audience: publico,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEmUtc,
            signingCredentials: credenciais);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AuthResposta(
            UsuarioId: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Papel: usuario.Papel,
            Token: tokenString,
            ExpiraEmUtc: expiraEmUtc);
    }
}
