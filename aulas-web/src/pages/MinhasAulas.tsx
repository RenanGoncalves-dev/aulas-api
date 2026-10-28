import { useCallback, useEffect, useState, type FormEvent } from 'react';
import {
  AlertCircle,
  Calendar,
  CheckCircle2,
  Clock,
  RefreshCw,
  ShieldAlert,
  User as UserIcon,
  X,
  XCircle,
} from 'lucide-react';
import { api, ErroApi } from '../services/api';
import type { AulaMarcada, Usuario } from '../types';
import './MinhasAulas.css';

interface MinhasAulasProps {
  usuario: Usuario;
}

type FiltroStatus = 'todas' | 'Agendada' | 'Cancelada';

function formatarDataHoraLocal(isoUtc: string): string {
  const data = new Date(isoUtc);
  if (Number.isNaN(data.getTime())) return isoUtc;
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(data);
}

export function MinhasAulas({ usuario }: MinhasAulasProps) {
  const [aulas, setAulas] = useState<AulaMarcada[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [sucesso, setSucesso] = useState<string | null>(null);

  const [filtroStatus, setFiltroStatus] = useState<FiltroStatus>('todas');
  const [filtroAluno, setFiltroAluno] = useState('');

  // Secção para testar proteção IDOR (403 Forbidden) ao tentar cancelar qualquer ID de aula
  const [idTesteIdor, setIdTesteIdor] = useState('');
  const [cancelandoId, setCancelandoId] = useState<number | null>(null);

  const ehAdmin = usuario.papel.toLowerCase() === 'admin';

  const carregarAulas = useCallback(async () => {
    setCarregando(true);
    setErro(null);
    try {
      const lista = await api.aulas.listar();
      setAulas(lista);
    } catch (err) {
      if (err instanceof ErroApi) {
        setErro(`${err.titulo} (${err.status}): ${err.message}`);
      } else {
        setErro('Não foi possível carregar as aulas.');
      }
    } finally {
      setCarregando(false);
    }
  }, []);

  useEffect(() => {
    void carregarAulas();
  }, [carregarAulas]);

  const handleCancelarAula = async (id: number) => {
    setErro(null);
    setSucesso(null);
    setCancelandoId(id);

    try {
      await api.aulas.cancelar(id);
      setSucesso(`Aula #${id} cancelada com sucesso.`);
      await carregarAulas();
    } catch (err) {
      if (err instanceof ErroApi) {
        setErro(`${err.titulo} (HTTP ${err.status}): ${err.message}`);
      } else {
        setErro('Erro inesperado ao cancelar a aula.');
      }
    } finally {
      setCancelandoId(null);
    }
  };

  const handleTesteIdorSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const idNum = Number(idTesteIdor);
    if (!Number.isInteger(idNum) || idNum <= 0) {
      setErro('Informe um ID numérico válido para testar o cancelamento.');
      return;
    }
    await handleCancelarAula(idNum);
  };

  const aulasFiltradas = aulas.filter((aula) => {
    if (filtroStatus !== 'todas' && aula.status !== filtroStatus) {
      return false;
    }
    if (ehAdmin && filtroAluno.trim()) {
      const termo = filtroAluno.trim().toLowerCase();
      return (
        aula.usuarioNome.toLowerCase().includes(termo) ||
        aula.professorNome.toLowerCase().includes(termo) ||
        aula.professorMateria.toLowerCase().includes(termo)
      );
    }
    return true;
  });

  return (
    <section className="aulas-page">
      <div className="aulas-topo">
        <div>
          <h1 className="aulas-topo__titulo">
            {ehAdmin ? 'Painel Geral de Aulas (Visão Admin)' : 'Minhas Aulas Agendadas'}
          </h1>
          <p className="aulas-topo__subtitulo">
            {ehAdmin
              ? 'Como administrador, visualiza as aulas de todos os clientes e pode gerir qualquer agendamento.'
              : 'Visualize os seus horários agendados em UTC (convertidos para o seu fuso local) ou cancele quando necessário.'}
          </p>
        </div>

        <button
          type="button"
          className="aulas-botao-atualizar"
          onClick={() => void carregarAulas()}
          disabled={carregando}
        >
          <RefreshCw size={16} />
          <span>Atualizar</span>
        </button>
      </div>

      {/* Filtros e Simulador IDOR */}
      <div className="aulas-painel-controles">
        <div className="aulas-filtros">
          <div className="aulas-abas-status">
            {(['todas', 'Agendada', 'Cancelada'] as FiltroStatus[]).map((st) => (
              <button
                key={st}
                type="button"
                className={`aulas-aba-btn ${filtroStatus === st ? 'aulas-aba-btn--ativo' : ''}`}
                onClick={() => setFiltroStatus(st)}
              >
                {st === 'todas' ? 'Todas' : st === 'Agendada' ? 'Agendadas' : 'Canceladas'}
              </button>
            ))}
          </div>

          {ehAdmin && (
            <input
              type="search"
              placeholder="Filtrar por aluno, professor ou matéria..."
              value={filtroAluno}
              onChange={(e) => setFiltroAluno(e.target.value)}
              className="aulas-input-filtro"
            />
          )}
        </div>

        <form className="aulas-idor-box" onSubmit={handleTesteIdorSubmit}>
          <div className="aulas-idor-box__label">
            <ShieldAlert size={16} />
            <span>Testar proteção IDOR (DELETE /api/aulas/&#123;id&#125;):</span>
          </div>
          <div className="aulas-idor-box__controles">
            <input
              type="number"
              min="1"
              placeholder="ID da aula (ex.: 3)"
              value={idTesteIdor}
              onChange={(e) => setIdTesteIdor(e.target.value)}
              className="aulas-idor-box__input"
            />
            <button type="submit" className="aulas-idor-box__botao">
              Tentar Cancelar ID
            </button>
          </div>
        </form>
      </div>

      {sucesso && (
        <div className="aulas-alerta aulas-alerta--sucesso" role="status">
          <CheckCircle2 size={18} />
          <span>{sucesso}</span>
          <button
            type="button"
            className="aulas-alerta__fechar"
            onClick={() => setSucesso(null)}
          >
            <X size={15} />
          </button>
        </div>
      )}

      {erro && (
        <div className="aulas-alerta aulas-alerta--erro" role="alert">
          <AlertCircle size={18} />
          <span>{erro}</span>
          <button type="button" className="aulas-alerta__fechar" onClick={() => setErro(null)}>
            <X size={15} />
          </button>
        </div>
      )}

      {carregando ? (
        <div className="aulas-vazio">A carregar agendamentos...</div>
      ) : aulasFiltradas.length === 0 ? (
        <div className="aulas-vazio">
          Nenhuma aula encontrada para o filtro selecionado.
        </div>
      ) : (
        <div className="aulas-lista">
          {aulasFiltradas.map((aula) => {
            const estaCancelada = aula.status === 'Cancelada';
            return (
              <article
                key={aula.id}
                className={`aula-item ${estaCancelada ? 'aula-item--cancelada' : ''}`}
              >
                <div className="aula-item__principal">
                  <div className="aula-item__badges">
                    <span className="aula-item__id">Aula #{aula.id}</span>
                    <span className="aula-item__materia">{aula.professorMateria}</span>
                    <span
                      className={`aula-item__status ${
                        estaCancelada
                          ? 'aula-item__status--cancelada'
                          : 'aula-item__status--agendada'
                      }`}
                    >
                      {aula.status}
                    </span>
                  </div>

                  <h2 className="aula-item__professor">Prof. {aula.professorNome}</h2>

                  <div className="aula-item__horarios">
                    <span className="aula-item__meta">
                      <Calendar size={15} />
                      Início: <strong>{formatarDataHoraLocal(aula.dataHoraInicioUtc)}</strong>
                    </span>
                    <span className="aula-item__meta">
                      <Clock size={15} />
                      Fim: <strong>{formatarDataHoraLocal(aula.dataHoraFimUtc)}</strong>
                    </span>
                    <span className="aula-item__meta">
                      <UserIcon size={15} />
                      Aluno: <strong>{aula.usuarioNome || `ID #${aula.usuarioId}`}</strong>
                    </span>
                  </div>

                  {aula.observacoes && (
                    <p className="aula-item__obs">“{aula.observacoes}”</p>
                  )}
                </div>

                <div className="aula-item__acoes">
                  {!estaCancelada && (
                    <button
                      type="button"
                      className="aula-item__botao-cancelar"
                      disabled={cancelandoId === aula.id}
                      onClick={() => void handleCancelarAula(aula.id)}
                    >
                      <XCircle size={16} />
                      <span>
                        {cancelandoId === aula.id ? 'A cancelar...' : 'Cancelar Aula'}
                      </span>
                    </button>
                  )}
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}

