# Aulas Particulares API
**Nicho:** Aulas particulares (música, idiomas, reforço) · **Aluno:** Renan Bernardo Gonçalves (@RenanGoncalves-dev)
**API no ar:** https://aulas-api.onrender.com/swagger
**Saúde e versão:** https://aulas-api.onrender.com/health · https://aulas-api.onrender.com/api/versao
**Front-end:** https://aulas-web.onrender.com

## O que a API faz
Plataforma de gestão de aulas particulares conectando alunos e professores de música, idiomas e reforço escolar. O sistema gerencia utilizadores (com papéis de Admin e Cliente), perfis de professores especializados, e o agendamento de aulas. 
- **Regra de Negócio (409 Conflict):** O sistema impede o agendamento de uma aula para um professor caso já exista outra aula agendada exatamente no mesmo horário/data, retornando o código HTTP 409 Conflict.

## Recursos e endpoints
| Verbo | Rota | Quem pode | Respostas |
|---|---|---|---|
| GET | /api/professores?busca=&pagina=&tamanho= | todos | 200 |
| GET | /api/professores/{id} | todos | 200, 404 |
| POST | /api/professores | admin | 201, 400, 401, 403 |
| PUT | /api/professores/{id} | admin | 200, 400, 401, 403, 404 |
| DELETE | /api/professores/{id} | admin | 204, 401, 403, 404, 409 |
| POST | /api/auth/registro | todos | 201, 400, 409 |
| POST | /api/auth/login | todos | 200, 400, 401 |
| GET | /api/aulas | autenticado | 200, 401 |
| POST | /api/aulas | cliente/admin | 201, 400, 401, 404, 409 |
| DELETE | /api/aulas/{id} | dono/admin | 204, 401, 403, 404, 409 |

## Rotas para o corretor testar (sem token)
- Listagem paginada: /api/professores?pagina=1&tamanho=5
- Busca (exemplo que encontra algo): /api/professores?busca=Matemática
- Detalhe: /api/professores/1 · inexistente (404): /api/professores/9999
- Protegida (401 sem token): /api/aulas

## Contas de teste (fictícias)
- admin: admin@exemplo.com / Admin@2026SecurePassword
- cliente A: ana@exemplo.com / Senha123! (ou `Cliente@123` no seed padrão)
- cliente B: bruno@exemplo.com / Senha123! (ou `Cliente@123` no seed padrão)

## Banco de dados
- **Entidades:** Utilizador (`Usuario`), Professor (`Professor`), AulaMarcada (`AulaMarcada`).
- **Relação 1 para N:** Um Professor pode possuir várias AulasMarcadas (1 para N), e um Utilizador (cliente) pode agendar várias AulasMarcadas.
- **Regra do 409:** Implementada no serviço de agendamentos (`AulaMarcadaServico.cs` / `AulasService.cs`), validando conflitos de horários na tabela de agendamentos antes de salvar.

## Como rodar no computador
- Configurar as variáveis de ambiente em `appsettings.Development.json` (ConnectionStrings__Padrao, Jwt__Segredo, Seed__AdminSenha).
- Executar migrações do EF Core: `dotnet ef database update`
- Executar a API: `dotnet run --project Aulas.Api`
- Executar os testes automatizados: `dotnet test`

## Front-end
- Desenvolvido em React + Vite + TypeScript utilizando CSS puro.
- Contém telas de autenticação (login/registro), listagem de professores com busca em tempo real, painel de agendamento de aulas e gestão/cancelamento de aulas marcadas.
- Configurado com a variável de CORS apontando para o front-end local e de produção.

## Como usei a IA
- IA usada: Gemini / Antigravity / Claude
- Os 3 pedidos (prompts) que mais ajudaram: 
  1. Criação do esqueleto da API em .NET com Clean Architecture / Entity Framework.
  2. Implementação das regras de validação de conflitos de horário com retorno 409.
  3. Geração da interface em React com Vite e CSS puro integrada via Axios.
- Um erro da IA que percebi e como corrigi: No arquivo `LoginRequisicao.cs`, a IA colocou anotações de validação (`[Required]`) diretamente nas propriedades do `record` do C#, gerando o erro `InvalidOperationException`. Corrigi movendo os atributos de validação para os parâmetros do construtor primário do record.
- O prompt usado para gerar o front: Solicitação de componentes limpos com React + TypeScript, Axios configurado com Bearer token do localStorage e estilização em CSS puro.
- O que aprendi: Consolidação da arquitetura REST em .NET, segurança com JWT, tratamento de exceções de concorrência e integração robusta com front-end moderno.

