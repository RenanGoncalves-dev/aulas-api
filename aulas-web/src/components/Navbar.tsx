import { BookOpen, Calendar, GraduationCap, LogIn, LogOut, Shield, User as UserIcon } from 'lucide-react';
import type { Usuario } from '../types';
import './Navbar.css';

export type AbaNavegacao = 'professores' | 'aulas' | 'login';

interface NavbarProps {
  usuario: Usuario | null;
  abaAtiva: AbaNavegacao;
  onMudarAba: (aba: AbaNavegacao) => void;
  onLogout: () => void;
}

export function Navbar({ usuario, abaAtiva, onMudarAba, onLogout }: NavbarProps) {
  const ehAdmin = usuario?.papel?.toLowerCase() === 'admin';

  return (
    <header className="navbar">
      <div className="navbar__container">
        <button
          type="button"
          className="navbar__marca"
          onClick={() => onMudarAba('professores')}
        >
          <span className="navbar__icone-marca">
            <GraduationCap size={22} />
          </span>
          <div className="navbar__textos-marca">
            <span className="navbar__titulo">Aulas Particulares</span>
            <span className="navbar__subtitulo">Música, Idiomas &amp; Reforço</span>
          </div>
        </button>

        <nav className="navbar__links" aria-label="Navegação principal">
          <button
            type="button"
            className={`navbar__link ${abaAtiva === 'professores' ? 'navbar__link--ativo' : ''}`}
            onClick={() => onMudarAba('professores')}
          >
            <BookOpen size={17} />
            <span>Professores</span>
          </button>

          {usuario && (
            <button
              type="button"
              className={`navbar__link ${abaAtiva === 'aulas' ? 'navbar__link--ativo' : ''}`}
              onClick={() => onMudarAba('aulas')}
            >
              <Calendar size={17} />
              <span>{ehAdmin ? 'Todas as Aulas' : 'Minhas Aulas'}</span>
            </button>
          )}
        </nav>

        <div className="navbar__acoes">
          {usuario ? (
            <div className="navbar__usuario">
              <div className="navbar__info-usuario">
                <span className="navbar__nome-usuario">{usuario.nome}</span>
                <span
                  className={`navbar__badge-papel ${
                    ehAdmin ? 'navbar__badge-papel--admin' : 'navbar__badge-papel--cliente'
                  }`}
                >
                  {ehAdmin ? <Shield size={12} /> : <UserIcon size={12} />}
                  {ehAdmin ? 'Admin' : 'Cliente'}
                </span>
              </div>

              <button
                type="button"
                className="navbar__botao-sair"
                onClick={onLogout}
                title="Terminar sessão"
              >
                <LogOut size={16} />
                <span>Sair</span>
              </button>
            </div>
          ) : (
            <button
              type="button"
              className={`navbar__botao-entrar ${abaAtiva === 'login' ? 'navbar__botao-entrar--ativo' : ''}`}
              onClick={() => onMudarAba('login')}
            >
              <LogIn size={16} />
              <span>Entrar / Registar</span>
            </button>
          )}
        </div>
      </div>
    </header>
  );
}

