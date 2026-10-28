export type PapelUsuario = 'admin' | 'cliente' | string;

export interface Usuario {
  usuarioId: number;
  nome: string;
  email: string;
  papel: PapelUsuario;
  token: string;
  expiraEmUtc: string;
}

export interface LoginDto {
  email: string;
  senha: string;
}

export interface RegistroDto {
  nome: string;
  email: string;
  senha: string;
}

export interface Professor {
  id: number;
  nome: string;
  materia: string;
  descricao: string;
  valorHora: number;
  duracaoMinutos: number;
  ativo: boolean;
  criadoEmUtc: string;
}

export interface ProfessorSalvarDto {
  nome: string;
  materia: string;
  descricao: string;
  valorHora: number;
  duracaoMinutos: number;
  ativo: boolean;
}

export interface ResultadoPaginado<T> {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalPaginas: number;
}

export type StatusAula = 'Agendada' | 'Cancelada' | string;

export interface AulaMarcada {
  id: number;
  professorId: number;
  professorNome: string;
  professorMateria: string;
  usuarioId: number;
  usuarioNome: string;
  dataHoraInicioUtc: string;
  dataHoraFimUtc: string;
  observacoes: string;
  status: StatusAula;
  criadoEmUtc: string;
}

export interface CriarAulaDto {
  professorId: number;
  dataHoraInicio: string;
  observacoes?: string;
}

export interface ProblemDetailsErro {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

