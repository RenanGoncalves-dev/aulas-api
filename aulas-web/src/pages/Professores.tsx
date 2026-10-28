import { useCallback, useEffect, useState, type FormEvent } from 'react';
import {
  AlertCircle,
  CalendarPlus,
  CheckCircle2,
  Clock,
  DollarSign,
  Plus,
  Search,
  Trash2,
  X,
} from 'lucide-react';
import { api, ErroApi } from '../services/api';
import type { Professor, Usuario } from '../types';
import './Professores.css';

interface ProfessoresProps {
  usuario: Usuario | null;
  onSolicitarLogin: () => void;
  onAulaCriada: () => void;
}

function sugerirDataHoraPadrao(): string {
  const amanha = new Date();
  amanha.setDate(amanha.getDate() + 1);
  amanha.setHours(14, 0, 0, 0);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${amanha.getFullYear()}-${pad(amanha.getMonth() + 1)}-${pad(amanha.getDate())}T${pad(
    amanha.getHours()
  )}:${pad(amanha.getMinutes())}`;
}

export function Professores({ usuario, onSolicitarLogin, onAulaCriada }: ProfessoresProps) {
  const [professores, setProfessores] = useState<Professor[]>([]);
  const [busca, setBusca] = useState('');
  const [pagina, setPagina] = useState(1);
  const [totalPaginas, setTotalPaginas] = useState(0);
  const [totalRegistros, setTotalRegistros] = useState(0);
  const [carregando, setCarregando] = useState(true);
  const [erroLista, setErroLista] = useState<string | null>(null);
  const [mensagemSucesso, setMensagemSucesso] = useState<string | null>(null);

  // Modal de agendamento de aula
  const [professorSelecionado, setProfessorSelecionado] = useState<Professor | null>(null);
  const [dataHoraInicio, setDataHoraInicio] = useState(sugerirDataHoraPadrao);
  const [observacoes, setObservacoes] = useState('');
  const [salvandoAula, setSalvandoAula] = useState(false);
  const [erroModalAula, setErroModalAula] = useState<string | null>(null);

  // Modal de novo professor (exclusivo Admin)
  const [modalNovoProfAberto, setModalNovoProfAberto] = useState(false);
  const [novoNome, setNovoNome] = useState('');
  const [novaMateria, setNovaMateria] = useState('');
  const [novaDescricao, setNovaDescricao] = useState('');
  const [novoValorHora, setNovoValorHora] = useState('95');
  const [novaDuracao, setNovaDuracao] = useState('60');
  const [salvandoProf, setSalvandoProf] = useState(false);
  const [erroModalProf, setErroModalProf] = useState<string | null>(null);

  const ehAdmin = usuario?.papel?.toLowerCase() === 'admin';

  const carregarProfessores = useCallback(async (termoBusca: string, pag: number) => {
    setCarregando(true);
    setErroLista(null);
    try {
      const resposta = await api.professores.listar(termoBusca, pag, 9);
      setProfessores(resposta.itens);
      setTotalPaginas(resposta.totalPaginas);
      setTotalRegistros(resposta.total);
    } catch (err) {
      if (err instanceof ErroApi) {
        setErroLista(`${err.titulo}: ${err.message}`);
      } else {
        setErroLista('Não foi possível carregar a lista de professores.');
      }
    } finally {
      setCarregando(false);
    }
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      void carregarProfessores(busca, pagina);
    }, 220);

    return () => window.clearTimeout(timer);
  }, [busca, pagina, carregarProfessores]);

  const abrirModalAgendamento = (prof: Professor) => {
    if (!usuario) {
      onSolicitarLogin();
      return;
    }
    setProfessorSelecionado(prof);
    setDataHoraInicio(sugerirDataHoraPadrao());
    setObservacoes('');
    setErroModalAula(null);
  };

  const handleConfirmarAgendamento = async (e: FormEvent) => {
    e.preventDefault();
    if (!professorSelecionado) return;

    setErroModalAula(null);
    setSalvandoAula(true);

    try {
      const dataConvertida = new Date(dataHoraInicio);
      if (Number.isNaN(dataConvertida.getTime())) {
        setErroModalAula('Informe uma data e hora válidas.');
        setSalvandoAula(false);
        return;
      }

      await api.aulas.criar({
        professorId: professorSelecionado.id,
        dataHoraInicio: dataConvertida.toISOString(),
        observacoes: observacoes.trim() || undefined,
      });

      setMensagemSucesso(`Aula com ${professorSelecionado.nome} agendada com sucesso!`);
      setProfessorSelecionado(null);
      onAulaCriada();
    } catch (err) {
      if (err instanceof ErroApi) {
        setErroModalAula(`${err.titulo}: ${err.message}`);
      } else {
        setErroModalAula('Não foi possível concluir o agendamento.');
      }
    } finally {
      setSalvandoAula(false);
    }
  };

  const handleCriarProfessor = async (e: FormEvent) => {
    e.preventDefault();
    setErroModalProf(null);
    setSalvandoProf(true);

    try {
      await api.professores.criar({
        nome: novoNome.trim(),
        materia: novaMateria.trim(),
        descricao: novaDescricao.trim(),
        valorHora: Number(novoValorHora),
        duracaoMinutos: Number(novaDuracao),
        ativo: true,
      });

      setModalNovoProfAberto(false);
      setNovoNome('');
      setNovaMateria('');
      setNovaDescricao('');
      setMensagemSucesso('Novo professor cadastrado com sucesso!');
      void carregarProfessores(busca, 1);
    } catch (err) {
      if (err instanceof ErroApi) {
        setErroModalProf(`${err.titulo}: ${err.message}`);
      } else {
        setErroModalProf('Erro ao cadastrar professor.');
      }
    } finally {
      setSalvandoProf(false);
    }
  };

  const handleExcluirProfessor = async (prof: Professor) => {
    if (!window.confirm(`Deseja realmente excluir o professor "${prof.nome}"?`)) return;
    setErroLista(null);
    setMensagemSucesso(null);

    try {
      await api.professores.excluir(prof.id);
      setMensagemSucesso(`Professor ${prof.nome} excluído com sucesso.`);
      void carregarProfessores(busca, pagina);
    } catch (err) {
      if (err instanceof ErroApi) {
        setErroLista(`${err.titulo}: ${err.message}`);
      } else {
        setErroLista('Erro ao excluir professor.');
      }
    }
  };

  return (
    <section className="professores-page">
      <div className="professores-topo">
        <div>
          <h1 className="professores-topo__titulo">Encontre o seu Professor Particular</h1>
          <p className="professores-topo__subtitulo">
            Explore especialistas em música, idiomas e reforço universitário ou escolar.
          </p>
        </div>

        {ehAdmin && (
          <button
            type="button"
            className="professores-botao-novo"
            onClick={() => {
              setErroModalProf(null);
              setModalNovoProfAberto(true);
            }}
          >
            <Plus size={18} />
            <span>Novo Professor</span>
          </button>
        )}
      </div>

      <div className="professores-busca-bar">
        <div className="professores-busca__input-wrap">
          <Search size={18} className="professores-busca__icone" />
          <input
            type="search"
            placeholder="Pesquisar em tempo real por nome ou matéria (ex.: Violão, Inglês, Cálculo)..."
            value={busca}
            onChange={(e) => {
              setBusca(e.target.value);
              setPagina(1);
            }}
            className="professores-busca__input"
          />
          {busca && (
            <button
              type="button"
              className="professores-busca__limpar"
              onClick={() => {
                setBusca('');
                setPagina(1);
              }}
              aria-label="Limpar pesquisa"
            >
              <X size={16} />
            </button>
          )}
        </div>

        <span className="professores-busca__contador">
          {totalRegistros} {totalRegistros === 1 ? 'professor encontrado' : 'professores encontrados'}
        </span>
      </div>

      {mensagemSucesso && (
        <div className="professores-alerta professores-alerta--sucesso" role="status">
          <CheckCircle2 size={18} />
          <span>{mensagemSucesso}</span>
          <button
            type="button"
            className="professores-alerta__fechar"
            onClick={() => setMensagemSucesso(null)}
          >
            <X size={15} />
          </button>
        </div>
      )}

      {erroLista && (
        <div className="professores-alerta professores-alerta--erro" role="alert">
          <AlertCircle size={18} />
          <span>{erroLista}</span>
          <button
            type="button"
            className="professores-alerta__fechar"
            onClick={() => setErroLista(null)}
          >
            <X size={15} />
          </button>
        </div>
      )}

      {carregando ? (
        <div className="professores-estado-vazio">A carregar catálogo de professores...</div>
      ) : professores.length === 0 ? (
        <div className="professores-estado-vazio">
          Nenhum professor encontrado para o termo <strong>"{busca}"</strong>.
        </div>
      ) : (
        <div className="professores-grid">
          {professores.map((prof) => (
            <article
              key={prof.id}
              className={`professor-card ${!prof.ativo ? 'professor-card--inativo' : ''}`}
            >
              <div className="professor-card__cabecalho">
                <span className="professor-card__materia">{prof.materia}</span>
                <span
                  className={`professor-card__status ${
                    prof.ativo ? 'professor-card__status--ativo' : 'professor-card__status--pausado'
                  }`}
                >
                  {prof.ativo ? 'Disponível' : 'Agenda pausada'}
                </span>
              </div>

              <h2 className="professor-card__nome">{prof.nome}</h2>
              <p className="professor-card__descricao">{prof.descricao}</p>

              <div className="professor-card__metricas">
                <div className="professor-card__metrica">
                  <DollarSign size={16} />
                  <span>
                    <strong>R$ {prof.valorHora.toFixed(2)}</strong> / aula
                  </span>
                </div>
                <div className="professor-card__metrica">
                  <Clock size={16} />
                  <span>{prof.duracaoMinutos} min</span>
                </div>
              </div>

              <div className="professor-card__acoes">
                <button
                  type="button"
                  className="professor-card__botao-agendar"
                  disabled={!prof.ativo}
                  onClick={() => abrirModalAgendamento(prof)}
                >
                  <CalendarPlus size={17} />
                  <span>{prof.ativo ? 'Agendar Aula' : 'Indisponível'}</span>
                </button>

                {ehAdmin && (
                  <button
                    type="button"
                    className="professor-card__botao-excluir"
                    onClick={() => handleExcluirProfessor(prof)}
                    title="Excluir professor (testa regra 409 se tiver aulas)"
                  >
                    <Trash2 size={16} />
                  </button>
                )}
              </div>
            </article>
          ))}
        </div>
      )}

      {totalPaginas > 1 && (
        <div className="professores-paginacao">
          <button
            type="button"
            disabled={pagina <= 1}
            onClick={() => setPagina((p) => Math.max(1, p - 1))}
            className="professores-paginacao__botao"
          >
            Anterior
          </button>
          <span className="professores-paginacao__info">
            Página {pagina} de {totalPaginas}
          </span>
          <button
            type="button"
            disabled={pagina >= totalPaginas}
            onClick={() => setPagina((p) => p + 1)}
            className="professores-paginacao__botao"
          >
            Seguinte
          </button>
        </div>
      )}

      {/* Modal de Agendamento de Aula */}
      {professorSelecionado && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal-caixa">
            <div className="modal-caixa__topo">
              <div>
                <h2 className="modal-caixa__titulo">Agendar Aula Particular</h2>
                <p className="modal-caixa__subtitulo">
                  {professorSelecionado.nome} — {professorSelecionado.materia} (
                  {professorSelecionado.duracaoMinutos} min)
                </p>
              </div>
              <button
                type="button"
                className="modal-caixa__fechar"
                onClick={() => setProfessorSelecionado(null)}
              >
                <X size={18} />
              </button>
            </div>

            {erroModalAula && (
              <div className="professores-alerta professores-alerta--erro" role="alert">
                <AlertCircle size={18} />
                <span>{erroModalAula}</span>
              </div>
            )}

            <form className="modal-form" onSubmit={handleConfirmarAgendamento}>
              <div className="modal-campo">
                <label htmlFor="data-inicio" className="modal-campo__label">
                  Data e hora de início (convertida automaticamente para UTC)
                </label>
                <input
                  id="data-inicio"
                  type="datetime-local"
                  required
                  value={dataHoraInicio}
                  onChange={(e) => setDataHoraInicio(e.target.value)}
                  className="modal-campo__input"
                />
              </div>

              <div className="modal-campo">
                <label htmlFor="observacoes-aula" className="modal-campo__label">
                  Objetivos ou observações para a aula (opcional)
                </label>
                <textarea
                  id="observacoes-aula"
                  rows={3}
                  maxLength={500}
                  placeholder="Ex.: Gostaria de focar em harmonia funcional ou preparação para entrevista..."
                  value={observacoes}
                  onChange={(e) => setObservacoes(e.target.value)}
                  className="modal-campo__textarea"
                />
              </div>

              <div className="modal-acoes">
                <button
                  type="button"
                  className="modal-botao modal-botao--secundario"
                  onClick={() => setProfessorSelecionado(null)}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="modal-botao modal-botao--primario"
                  disabled={salvandoAula}
                >
                  {salvandoAula ? 'A confirmar...' : 'Confirmar Agendamento'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal de Criação de Professor (Admin) */}
      {modalNovoProfAberto && (
        <div className="modal-backdrop" role="dialog" aria-modal="true">
          <div className="modal-caixa">
            <div className="modal-caixa__topo">
              <div>
                <h2 className="modal-caixa__titulo">Cadastrar Novo Professor</h2>
                <p className="modal-caixa__subtitulo">Operação exclusiva para Administradores</p>
              </div>
              <button
                type="button"
                className="modal-caixa__fechar"
                onClick={() => setModalNovoProfAberto(false)}
              >
                <X size={18} />
              </button>
            </div>

            {erroModalProf && (
              <div className="professores-alerta professores-alerta--erro" role="alert">
                <AlertCircle size={18} />
                <span>{erroModalProf}</span>
              </div>
            )}

            <form className="modal-form" onSubmit={handleCriarProfessor}>
              <div className="modal-campo">
                <label htmlFor="prof-nome" className="modal-campo__label">
                  Nome completo
                </label>
                <input
                  id="prof-nome"
                  type="text"
                  required
                  minLength={2}
                  maxLength={120}
                  value={novoNome}
                  onChange={(e) => setNovoNome(e.target.value)}
                  className="modal-campo__input"
                />
              </div>

              <div className="modal-campo">
                <label htmlFor="prof-materia" className="modal-campo__label">
                  Matéria / Especialidade
                </label>
                <input
                  id="prof-materia"
                  type="text"
                  required
                  minLength={2}
                  maxLength={80}
                  value={novaMateria}
                  onChange={(e) => setNovaMateria(e.target.value)}
                  className="modal-campo__input"
                />
              </div>

              <div className="modal-campo">
                <label htmlFor="prof-descricao" className="modal-campo__label">
                  Descrição e experiência
                </label>
                <textarea
                  id="prof-descricao"
                  rows={3}
                  required
                  minLength={5}
                  maxLength={600}
                  value={novaDescricao}
                  onChange={(e) => setNovaDescricao(e.target.value)}
                  className="modal-campo__textarea"
                />
              </div>

              <div className="modal-linha-dupla">
                <div className="modal-campo">
                  <label htmlFor="prof-valor" className="modal-campo__label">
                    Valor Hora (R$)
                  </label>
                  <input
                    id="prof-valor"
                    type="number"
                    step="0.01"
                    min="1"
                    required
                    value={novoValorHora}
                    onChange={(e) => setNovoValorHora(e.target.value)}
                    className="modal-campo__input"
                  />
                </div>

                <div className="modal-campo">
                  <label htmlFor="prof-duracao" className="modal-campo__label">
                    Duração (min)
                  </label>
                  <input
                    id="prof-duracao"
                    type="number"
                    min="15"
                    max="240"
                    required
                    value={novaDuracao}
                    onChange={(e) => setNovaDuracao(e.target.value)}
                    className="modal-campo__input"
                  />
                </div>
              </div>

              <div className="modal-acoes">
                <button
                  type="button"
                  className="modal-botao modal-botao--secundario"
                  onClick={() => setModalNovoProfAberto(false)}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="modal-botao modal-botao--primario"
                  disabled={salvandoProf}
                >
                  {salvandoProf ? 'A guardar...' : 'Salvar Professor'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </section>
  );
}

