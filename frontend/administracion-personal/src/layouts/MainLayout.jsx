import logo from "../assets/Logo.png";
import { NavLink, Outlet, useNavigate } from "react-router";
import { useAuth } from "../context/AuthContext";
import "./MainLayout.css";

function MainLayout() {
  const navigate = useNavigate();

  const {
    usuario,
    cerrarSesion,
  } = useAuth();

const handleCerrarSesion = () => {
  cerrarSesion();
};

  return (
    <div className="app-container">

      {/* HEADER */}
      <header className="app-header">
        <div className="app-logo">
          <div className="logo-icon">
  <img
    src={logo}
    alt="Logo Administración de Personal"
    className="layout-logo-img"
  />
</div>

          <div>
            <h1>Administración de Personal</h1>
            <span>Servicios Médicos S.A.</span>
          </div>
        </div>

        <div className="usuario-header">
          <div className="usuario-info">
            <span className="usuario-icon">👤</span>

            <div>
              <strong>
                {usuario?.nombreCompleto ||
                  usuario?.usuario ||
                  "Usuario"}
              </strong>

              <span className="usuario-rol">
                {usuario?.roles?.length > 0
                  ? usuario.roles.join(", ")
                  : "Usuario"}
              </span>
            </div>
          </div>

          <button
            className="btn-salir"
            onClick={handleCerrarSesion}
          >
            Cerrar sesión
          </button>
        </div>
      </header>

      {/* CUERPO */}
      <div className="app-body">

        {/* MENÚ LATERAL */}
        <aside className="sidebar">
          <nav className="sidebar-nav">

            <NavLink
              to="/bienvenida"
              className={({ isActive }) =>
                isActive
                  ? "nav-item active"
                  : "nav-item"
              }
            >
              Inicio
            </NavLink>

            <div className="sidebar-titulo">
              Recursos Humanos
            </div>

            {/*
              Estas rutas quedan preparadas para
              las páginas de los compañeros.
            */}

            <div className="nav-item nav-disabled">
              Puestos
            </div>

            <div className="nav-item nav-disabled">
              Oferentes
            </div>

          </nav>
        </aside>

        {/* CONTENIDO DE CADA PÁGINA */}
        <main className="app-content">
          <Outlet />
        </main>

      </div>

      {/* FOOTER */}
      <footer className="app-footer">
        <span>
          © 2026 Administración de Personal
        </span>

        <span>
          Servicios Médicos S.A.
        </span>
      </footer>

    </div>
  );
}

export default MainLayout;