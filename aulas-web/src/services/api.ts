import type {
  AulaMarcada,
  CriarAulaDto,
  LoginDto,
  ProblemDetailsErro,
  Professor,
  ProfessorSalvarDto,
  RegistroDto,
  ResultadoPaginado,
  Usuario,
} from '../types';

const API_BASE_URL = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/+$/, '') || 'https://aulas-api-pkfu.onrender.com';
const CHAVE_STORAGE = 'aulas_usuario_sessao';

export class ErroApi extends Error {
  public readonly status: number;
  public readonly titulo: string;

  constructor(mensagem: string, status: number, titulo = 'Erro na requisição') {
    super(mensagem);
    this.name = 'ErroApi';
    this.status = status;
    this.titulo = titulo;
  }
}

export function obterSessaoSalva(): Usuario | null {
  try {
    const raw = localStorage.getItem(CHAVE_STORAGE);
    if (!raw) return null;
    const usuario = JSON.parse(raw) as Usuario;
    if (!usuario?.token) return null;
    return usuario;
  } catch {
    return null;
  }
}

export function salvarSessao(usuario: Usuario): void {
  localStorage.setItem(CHAVE_STORAGE, JSON.stringify(usuario));
}

export function limparSessao(): void {
  localStorage.removeItem(CHAVE_STORAGE);
}

function extrairMensagemAmigavel(status: number, corpo: ProblemDetailsErro | null): { titulo: string; mensagem: string } {
  if (corpo) {
    if (corpo.errors && typeof corpo.errors === 'object') {
      const mensagensValidacao = Object.values(corpo.errors).flat().filter(Boolean);
      if (mensagensValidacao.length > 0) {
        return {
          titulo: corpo.title || 'Dados inválidos',
          mensagem: mensagensValidacao.join(' '),
        };
      }
    }

    if (corpo.detail) {
      return {
        titulo: corpo.title || 'Atenção',
        mensagem: corpo.detail,
      };
    }

    if (corpo.title) {
      return {
        titulo: corpo.title,
        mensagem: corpo.title,
      };
    }
  }

  switch (status) {
    case 400:
      return { titulo: 'Requisição inválida', mensagem: 'Verifique os campos preenchidos e tente novamente.' };
    case 401:
      return { titulo: 'Não autorizado', mensagem: 'A sua sessão expirou ou as credenciais são inválidas.' };
    case 403:
      return { titulo: 'Acesso negado (403)', mensagem: 'Não tem permissão para realizar esta operação neste recurso.' };
    case 404:
      return { titulo: 'Não encontrado', mensagem: 'O registo solicitado não foi encontrado no servidor.' };
    case 409:
      return { titulo: 'Conflito de agenda', mensagem: 'Operação bloqueada por conflito de regra de negócio.' };
    default:
      return { titulo: 'Erro de comunicação', mensagem: `Ocorreu um erro inesperado (código ${status}).` };
  }
}

async function requisicao<T>(caminho: string, opcoes: RequestInit = {}): Promise<T> {
  const sessao = obterSessaoSalva();
  const headers = new Headers(opcoes.headers);

  if (!headers.has('Content-Type') && opcoes.body) {
    headers.set('Content-Type', 'application/json');
  }
  headers.set('Accept', 'application/json, application/problem+json');

  if (sessao?.token) {
    headers.set('Authorization', `Bearer ${sessao.token}`);
  }

  let resposta: Response;
  try {
    resposta = await fetch(`${API_BASE_URL}${caminho}`, {
      ...opcoes,
      headers,
    });
  } catch {
    throw new ErroApi(
      `Não foi possível ligar ao servidor em ${API_BASE_URL}. Verifique se a API .NET está em execução.`,
      0,
      'Servidor indisponível'
    );
  }

  if (resposta.status === 204) {
    return undefined as T;
  }

  const texto = await resposta.text();
  let dados: unknown = null;
  if (texto) {
    try {
      dados = JSON.parse(texto);
    } catch {
      dados = null;
    }
  }

  if (!resposta.ok) {
    const { titulo, mensagem } = extrairMensagemAmigavel(
      resposta.status,
      dados as ProblemDetailsErro | null
    );
    throw new ErroApi(mensagem, resposta.status, titulo);
  }

  return dados as T;
}

export const api = {
  urlBase: API_BASE_URL,

  auth: {
    login: async (dto: LoginDto): Promise<Usuario> => {
      const usuario = await requisicao<Usuario>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(dto),
      });
      salvarSessao(usuario);
      return usuario;
    },

    registrar: async (dto: RegistroDto): Promise<Usuario> => {
      const usuario = await requisicao<Usuario>('/api/auth/registro', {
        method: 'POST',
        body: JSON.stringify(dto),
      });
      salvarSessao(usuario);
      return usuario;
    },
  },

  professores: {
    listar: async (busca = '', pagina = 1, tamanho = 12): Promise<ResultadoPaginado<Professor>> => {
      const params = new URLSearchParams({
        pagina: String(pagina),
        tamanho: String(tamanho),
      });
      if (busca.trim()) {
        params.set('busca', busca.trim());
      }
      return requisicao<ResultadoPaginado<Professor>>(`/api/professores?${params.toString()}`);
    },

    obterPorId: async (id: number): Promise<Professor> => {
      return requisicao<Professor>(`/api/professores/${id}`);
    },

    criar: async (dto: ProfessorSalvarDto): Promise<Professor> => {
      return requisicao<Professor>('/api/professores', {
        method: 'POST',
        body: JSON.stringify(dto),
      });
    },

    atualizar: async (id: number, dto: ProfessorSalvarDto): Promise<Professor> => {
      return requisicao<Professor>(`/api/professores/${id}`, {
        method: 'PUT',
        body: JSON.stringify(dto),
      });
    },

    excluir: async (id: number): Promise<void> => {
      return requisicao<void>(`/api/professores/${id}`, {
        method: 'DELETE',
      });
    },
  },

  aulas: {
    listar: async (): Promise<AulaMarcada[]> => {
      return requisicao<AulaMarcada[]>('/api/aulas');
    },

    criar: async (dto: CriarAulaDto): Promise<AulaMarcada> => {
      return requisicao<AulaMarcada>('/api/aulas', {
        method: 'POST',
        body: JSON.stringify(dto),
      });
    },

    cancelar: async (id: number): Promise<void> => {
      return requisicao<void>(`/api/aulas/${id}`, {
        method: 'DELETE',
      });
    },
  },
};

