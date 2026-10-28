using Aulas.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Aulas.Api.Dados;

public static class InicializadorBanco
{
    public static async Task SemearAsync(AppDbContext db, IConfiguration configuration, ILogger logger)
    {
        var adminSenha = configuration["Seed:AdminSenha"];
        if (string.IsNullOrWhiteSpace(adminSenha))
        {
            throw new InvalidOperationException(
                "A senha do administrador para o seed ('Seed:AdminSenha' / variável 'Seed__AdminSenha') não foi configurada.");
        }

        var agoraUtc = DateTime.UtcNow;

        // 1. Seed idempotente do Admin
        var admin = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@exemplo.com");
        if (admin is null)
        {
            admin = new Usuario(
                nome: "Administrador do Sistema",
                email: "admin@exemplo.com",
                senhaHash: BCrypt.Net.BCrypt.HashPassword(adminSenha),
                papel: Usuario.PapelAdmin,
                criadoEmUtc: agoraUtc);
            db.Usuarios.Add(admin);
            logger.LogInformation("Usuário admin@exemplo.com criado no seed.");
        }

        // 2. Seed idempotente dos 2 clientes de teste
        var ana = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "ana@exemplo.com");
        if (ana is null)
        {
            ana = new Usuario(
                nome: "Ana Beatriz Souza",
                email: "ana@exemplo.com",
                senhaHash: BCrypt.Net.BCrypt.HashPassword("Cliente@123"),
                papel: Usuario.PapelCliente,
                criadoEmUtc: agoraUtc);
            db.Usuarios.Add(ana);
            logger.LogInformation("Cliente ana@exemplo.com criada no seed.");
        }

        var bruno = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == "bruno@exemplo.com");
        if (bruno is null)
        {
            bruno = new Usuario(
                nome: "Bruno Henrique Lima",
                email: "bruno@exemplo.com",
                senhaHash: BCrypt.Net.BCrypt.HashPassword("Cliente@123"),
                papel: Usuario.PapelCliente,
                criadoEmUtc: agoraUtc);
            db.Usuarios.Add(bruno);
            logger.LogInformation("Cliente bruno@exemplo.com criado no seed.");
        }

        await db.SaveChangesAsync();

        // 3. Seed idempotente de Professores (pelo menos 8 registros realistas abrangendo música, idiomas e reforço)
        if (!await db.Professores.AnyAsync())
        {
            var professores = new List<Professor>
            {
                new(
                    nome: "Clara Mendonça",
                    materia: "Violão e Guitarra",
                    descricao: "Formada em Música pela UFMG, leciona violão popular, harmonia funcional e improvisação na guitarra para iniciantes e intermediários.",
                    valorHora: 95.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Rafael Vasconcelos",
                    materia: "Piano Clássico e Teclado",
                    descricao: "Pianista com 12 anos de experiência em leitura de partitura, técnica pianística, acompanhamento e preparação para conservatório.",
                    valorHora: 120.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Marina Takahashi",
                    materia: "Canto e Técnica Vocal",
                    descricao: "Fonoaudióloga e preparadora vocal focada em respiração, afinação, projeção sem esforço e repertório MPB e pop.",
                    valorHora: 110.00m,
                    duracaoMinutos: 50,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Lucas Alencar",
                    materia: "Inglês para Conversação",
                    descricao: "Certificado Cambridge C2, aulas focadas em fluência para entrevistas técnicas de TI, viagens internacionais e reuniões em inglês.",
                    valorHora: 105.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Valeria Morales",
                    materia: "Espanhol Nativo e DELE",
                    descricao: "Professora nativa de Buenos Aires, especialista em conversação cotidiana, espanhol corporativo e exames DELE/SIELE.",
                    valorHora: 90.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Jean-Luc Moreau",
                    materia: "Francês Instrumental e Conversação",
                    descricao: "Mestre em Letras Francesas, prepara alunos para intercâmbio acadêmico na França e exames DELF B1/B2.",
                    valorHora: 115.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Henrique Cavalcanti",
                    materia: "Reforço de Matemática e Cálculo I",
                    descricao: "Engenheiro civil com didática passo a passo para ensino médio, ENEM, pré-cálculo, limites, derivadas e integrais.",
                    valorHora: 85.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Camila Nogueira",
                    materia: "Reforço de Física e Química",
                    descricao: "Licenciada em Física pela USP, resolução orientada de exercícios de mecânica, eletricidade e estequiometria para vestibulares.",
                    valorHora: 88.00m,
                    duracaoMinutos: 60,
                    ativo: true,
                    criadoEmUtc: agoraUtc),
                new(
                    nome: "Otávio Peixoto",
                    materia: "Bateria e Percussão",
                    descricao: "Baterista de estúdio, aulas de coordenação motora, rudimentos e levadas de rock, jazz e ritmos brasileiros (agenda pausada).",
                    valorHora: 100.00m,
                    duracaoMinutos: 60,
                    ativo: false,
                    criadoEmUtc: agoraUtc)
            };

            db.Professores.AddRange(professores);
            await db.SaveChangesAsync();
            logger.LogInformation("Seed de {Quantidade} professores concluído.", professores.Count);
        }

        // 4. Seed idempotente de AulasMarcadas para ana@exemplo.com e bruno@exemplo.com
        if (!await db.AulasMarcadas.AnyAsync())
        {
            var todosProfessores = await db.Professores
                .Where(p => p.Ativo)
                .OrderBy(p => p.Id)
                .ToListAsync();

            if (todosProfessores.Count >= 3)
            {
                var baseDataUtc = new DateTime(
                    agoraUtc.Year,
                    agoraUtc.Month,
                    agoraUtc.Day,
                    14, 0, 0,
                    DateTimeKind.Utc).AddDays(3);

                var aulas = new List<AulaMarcada>
                {
                    new(
                        professorId: todosProfessores[0].Id,
                        usuarioId: ana.Id,
                        dataHoraInicioUtc: baseDataUtc,
                        dataHoraFimUtc: baseDataUtc.AddMinutes(todosProfessores[0].DuracaoMinutos),
                        observacoes: "Primeira aula prática de violão — revisar acordes maiores e pestana.",
                        criadoEmUtc: agoraUtc),
                    new(
                        professorId: todosProfessores[3 % todosProfessores.Count].Id,
                        usuarioId: ana.Id,
                        dataHoraInicioUtc: baseDataUtc.AddDays(1).AddHours(2),
                        dataHoraFimUtc: baseDataUtc.AddDays(1).AddHours(3),
                        observacoes: "Simulação de entrevista técnica em inglês.",
                        criadoEmUtc: agoraUtc),
                    new(
                        professorId: todosProfessores[1].Id,
                        usuarioId: bruno.Id,
                        dataHoraInicioUtc: baseDataUtc.AddDays(2),
                        dataHoraFimUtc: baseDataUtc.AddDays(2).AddMinutes(todosProfessores[1].DuracaoMinutos),
                        observacoes: "Estudo de leitura à primeira vista e escalas menores no piano.",
                        criadoEmUtc: agoraUtc),
                    new(
                        professorId: todosProfessores[6 % todosProfessores.Count].Id,
                        usuarioId: bruno.Id,
                        dataHoraInicioUtc: baseDataUtc.AddDays(4).AddHours(1),
                        dataHoraFimUtc: baseDataUtc.AddDays(4).AddHours(2),
                        observacoes: "Revisão de derivadas e regra da cadeia para prova de Cálculo I.",
                        criadoEmUtc: agoraUtc)
                };

                db.AulasMarcadas.AddRange(aulas);
                await db.SaveChangesAsync();
                logger.LogInformation("Seed de {Quantidade} aulas marcadas concluído.", aulas.Count);
            }
        }
    }
}
