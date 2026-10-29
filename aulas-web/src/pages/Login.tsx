import { useState, type FormEvent } from 'react';
import {
  AlertCircle,
  Eye,
  EyeOff,
  KeyRound,
  LogIn,
  Mail,
  Sparkles,
  User as UserIcon,
  UserPlus,
} from 'lucide-react';
import { api, ErroApi } from '../services/api';
import type { Usuario } from '../types';
import './Login.css';

interface LoginProps {
  onAutenticado: (usuario: Usuario) => void;
}

type ModoAuth = 'login' | 'registro';

export function Login({ onAutenticado }: LoginProps) {
  const [modo, setModo] = useState<ModoAuth>('login');
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const alternarModo = (novoModo: ModoAuth) => {
    setModo(novoModo);
    setErro(null);
  };

  const preencherDemo = (emailDemo: string, senhaDemo: string) => {
    setModo('login');
    setEmail(emailDemo);
    setSenha(senhaDemo);
    setErro(null);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErro(null);
    setCarregando(true);

    try {
      let usuario: Usuario;
      if (modo === 'login') {
        usuario = await api.auth.login({ email: email.trim(), senha });
      } else {
        usuario = await api.auth.registrar({
          nome: nome.trim(),
          email: email.trim(),
          senha,
        });
      }
      onAutenticado(usuario);
    } catch (err) {
      if (err instanceof ErroApi) {
        setErro(`${err.titulo}: ${err.message}`);
      } else {
        setErro('Ocorreu um erro inesperado ao autenticar.');
      }
    } finally {
      setCarregando(false);
    }
  };

  return (
    <section className="login-page">
      <div className="login-card">
        <div className="login-card__cabecalho">
          <span className="login-card__badge">
            <Sparkles size={14} />
            Acesso à Plataforma
          </span>
          <h1 className="login-card__titulo">
            {modo === 'login' ? 'Bem-vindo de volta' : 'Criar conta de Aluno'}
          </h1>
          <p className="login-card__subtitulo">
            {modo === 'login'
              ? 'Entre com a sua conta para agendar ou gerenciar suas aulas particulares.'
              : 'Cadastre-se gratuitamente para marcar aulas com nossos professores.'}
          </p>
        </div>

        <div className="login-abas" role="tablist">
          <button
            type="button"
            role="tab"
            aria-selected={modo === 'login'}
            className={`login-abas__botao ${modo === 'login' ? 'login-abas__botao--ativo' : ''}`}
            onClick={() => alternarModo('login')}
          >
            <LogIn size={16} />
            <span>Entrar</span>
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={modo === 'registro'}
            className={`login-abas__botao ${modo === 'registro' ? 'login-abas__botao--ativo' : ''}`}
            onClick={() => alternarModo('registro')}
          >
            <UserPlus size={16} />
            <span>Cadastrar-se</span>
          </button>
        </div>

        {erro && (
          <div className="login-alerta" role="alert">
            <AlertCircle size={18} />
            <span>{erro}</span>
          </div>
        )}

        <form className="login-form" onSubmit={handleSubmit}>
          {modo === 'registro' && (
            <div className="login-campo">
              <label htmlFor="campo-nome" className="login-campo__label">
                Nome completo
              </label>
              <div className="login-campo__input-wrap">
                <UserIcon size={17} className="login-campo__icone" />
                <input
                  id="campo-nome"
                  type="text"
                  required
                  minLength={2}
                  maxLength={120}
                  placeholder="Ex.: Ana Beatriz Souza"
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  className="login-campo__input"
                />
              </div>
            </div>
          )}

          <div className="login-campo">
            <label htmlFor="campo-email" className="login-campo__label">
              E-mail
            </label>
            <div className="login-campo__input-wrap">
              <Mail size={17} className="login-campo__icone" />
              <input
                id="campo-email"
                type="email"
                required
                maxLength={160}
                placeholder="seu.email@exemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="login-campo__input"
              />
            </div>
          </div>

          <div className="login-campo">
            <label htmlFor="campo-senha" className="login-campo__label">
              Senha
            </label>
            <div className="login-campo__input-wrap">
              <KeyRound size={17} className="login-campo__icone" />
              <input
                id="campo-senha"
                type={mostrarSenha ? 'text' : 'password'}
                required
                minLength={6}
                placeholder="Digite sua senha (mín. 6 caracteres)"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                className="login-campo__input login-campo__input--senha"
              />
              <button
                type="button"
                className="login-campo__toggle-senha"
                onClick={() => setMostrarSenha((v) => !v)}
                aria-label={mostrarSenha ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {mostrarSenha ? <EyeOff size={17} /> : <Eye size={17} />}
              </button>
            </div>
          </div>

          <button type="submit" className="login-botao-submit" disabled={carregando}>
            {carregando
              ? 'Autenticando...'
              : modo === 'login'
              ? 'Entrar na plataforma'
              : 'Concluir cadastro'}
          </button>
        </form>

        <div className="login-demos">
          <span className="login-demos__titulo">Contas de teste rápidas (1 clique):</span>
          <div className="login-demos__botoes">
            <button
              type="button"
              className="login-demos__chip"
              onClick={() => preencherDemo('ana@exemplo.com', 'Cliente@123')}
            >
              Cliente Ana
            </button>
            <button
              type="button"
              className="login-demos__chip"
              onClick={() => preencherDemo('bruno@exemplo.com', 'Cliente@123')}
            >
              Cliente Bruno
            </button>
            <button
              type="button"
              className="login-demos__chip login-demos__chip--admin"
              onClick={() => preencherDemo('admin@exemplo.com', 'Admin@2026SecurePassword')}
            >
              Admin
            </button>
          </div>
        </div>
      </div>
    </section>
  );
}
