using System.Text;
using Aulas.Api.Dados;
using Aulas.Api.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Padrao");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A connection string 'Padrao' não foi configurada. Defina a variável de ambiente 'ConnectionStrings__Padrao' no formato Npgsql (Host=...;Database=...;Username=...;Password=...;).");
}

if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Formato de connection string inválido. Não utilize URI 'postgresql://'; utilize o formato chave-valor do Npgsql (Host=...;Database=...;Username=...;Password=...;).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddProblemDetails();

var jwtSegredo = builder.Configuration["Jwt:Segredo"];
if (string.IsNullOrWhiteSpace(jwtSegredo) || Encoding.UTF8.GetByteCount(jwtSegredo) < 32)
{
    throw new InvalidOperationException(
        "O segredo JWT ('Jwt:Segredo' / variável 'Jwt__Segredo') deve estar configurado e possuir pelo menos 32 bytes.");
}

var jwtEmissor = builder.Configuration["Jwt:Emissor"] ?? "Aulas.Api";
var jwtPublico = builder.Configuration["Jwt:Publico"] ?? "Aulas.Clientes";
var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSegredo));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtEmissor,
            ValidateAudience = true,
            ValidAudience = jwtPublico,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = chaveSimetrica,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthServico, AuthServico>();
builder.Services.AddScoped<IProfessorServico, ProfessorServico>();
builder.Services.AddScoped<IAulaMarcadaServico, AulaMarcadaServico>();

var corsOrigensConfig = builder.Configuration["Cors:Origens"];
var origens = string.IsNullOrWhiteSpace(corsOrigensConfig)
    ? new[] { "http://localhost:5173" }
    : corsOrigensConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPadrao", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Location");
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aulas Particulares API",
        Version = "v1",
        Description = "Projeto escolar — empresa fictícia"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer ')."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await InicializadorBanco.SemearAsync(db, app.Configuration, app.Logger);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aulas Particulares API v1");
});

app.UseCors("CorsPadrao");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.MapHealthChecks("/health");

app.MapGet("/api/versao", (IHostEnvironment env) => Results.Ok(new
{
    commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local",
    ambiente = env.EnvironmentName
}));

await app.RunAsync();

public partial class Program { }
