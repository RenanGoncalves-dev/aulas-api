import { useEffect, useState } from 'react';
import { Navbar, type AbaNavegacao } from './components/Navbar';
import { Login } from './pages/Login';
import { MinhasAulas } from './pages/MinhasAulas';
import { Professores } from './pages/Professores';
import { api, limparSessao, obterSessaoSalva } from './services/api';
import type { Usuario } from './types';
import './App.css';

const CHAVE_TEMA = 'aulas_tema_preferido';

export function App() {
  const [usuario, setUsuario] = useState<Usuario | null>(() => obterSessaoSalva());
  const [abaAtiva, setAbaAtiva] = useState<AbaNavegacao>('professores');
  const [modoEscuro, setModoEscuro] = useState<boolean>(() => {
    const salvo = localStorage.getItem(CHAVE_TEMA);
    if (salvo === 'dark' || salvo === 'light') {
      return salvo === 'dark';
    }
    return true; // Inicia no modo escuro moderno por padrão
  });

  useEffect(() => {
    const tema = modoEscuro ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', tema);
    localStorage.setItem(CHAVE_TEMA, tema);
  }, [modoEscuro]);

  const handleAutenticado = (novoUsuario: Usuario) => {
    setUsuario(novoUsuario);
    setAbaAtiva('professores');
  };

  const handleLogout = () => {
    limparSessao();
    setUsuario(null);
    setAbaAtiva('login');
  };

  const handleMudarAba = (novaAba: AbaNavegacao) => {
    if (novaAba === 'aulas' && !usuario) {
      setAbaAtiva('login');
      return;
    }
    setAbaAtiva(novaAba);
  };

  return (
    <div className="app-layout">
      <Navbar
        usuario={usuario}
        abaAtiva={abaAtiva}
        modoEscuro={modoEscuro}
        onAlternarTema={() => setModoEscuro((prev) => !prev)}
        onMudarAba={handleMudarAba}
        onLogout={handleLogout}
      />

      <main className="app-conteudo">
        {abaAtiva === 'login' && <Login onAutenticado={handleAutenticado} />}

        {abaAtiva === 'professores' && (
          <Professores
            usuario={usuario}
            onSolicitarLogin={() => setAbaAtiva('login')}
            onAulaCriada={() => setAbaAtiva('aulas')}
          />
        )}

        {abaAtiva === 'aulas' &&
          (usuario ? (
            <MinhasAulas usuario={usuario} />
          ) : (
            <Login onAutenticado={handleAutenticado} />
          ))}
      </main>

      <footer className="app-rodape">
        <div className="app-rodape__container">
          <span>Aulas Particulares — Plataforma Web (.NET 10 + React + TypeScript)</span>
          <span className="app-rodape__api">API: {api.urlBase}</span>
        </div>
      </footer>
    </div>
  );
}

export default App;
