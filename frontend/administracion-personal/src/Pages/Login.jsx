import {
  useEffect,
  useState,
} from "react";

import {
  useLocation,
  useNavigate,
} from "react-router";

import { login } from "../services/authService";
import { useAuth } from "../context/AuthContext";

function Login() {
  const navigate = useNavigate();
  const location = useLocation();

    const {
    iniciarSesion,
    limpiarMotivoCierre,
    } = useAuth();


  const [usuario, setUsuario] =
    useState("");

  const [password, setPassword] =
    useState("");

  const [mensaje, setMensaje] =
    useState("");

  const [cargando, setCargando] =
    useState(false);

  // ==========================================
  // MENSAJES RECIBIDOS DESDE OTRAS RUTAS
  // ==========================================

 useEffect(() => {
  if (location.state?.mensaje) {
    setMensaje(
      location.state.mensaje
    );

    navigate("/login", {
      replace: true,
      state: null,
    });
  }

  limpiarMotivoCierre();

}, [
  location.state,
  navigate,
  limpiarMotivoCierre,
]);
  // ==========================================
  // LOGIN
  // ==========================================

  const handleSubmit = async (event) => {
    event.preventDefault();

    setMensaje("");

    // Validación requerida por la HU
    if (
      !usuario.trim() ||
      !password.trim()
    ) {
      setMensaje(
        "Usuario y/o contraseña incorrectos."
      );

      return;
    }

    try {
      setCargando(true);

      const respuesta =
        await login(
          usuario.trim(),
          password
        );

      // Guarda token y datos del usuario
      // mediante AuthContext.
      iniciarSesion(
        respuesta.data
      );

      // Login exitoso
      navigate(
        "/bienvenida",
        {
          replace: true,
        }
      );

    } catch (error) {
      setMensaje(
        error?.message ||
          "Usuario y/o contraseña incorrectos."
      );

    } finally {
      setCargando(false);
    }
  };

  // ==========================================
  // VISTA
  // ==========================================

  return (
    <div className="login-page">

      <div className="login-card">

        <div className="login-logo">
          <div className="login-logo-icon">
            AP
          </div>
        </div>

        <h1>
          Administración de Personal
        </h1>

        <p className="login-empresa">
          Servicios Médicos S.A.
        </p>

        <h2>
          Iniciar sesión
        </h2>

        {/* MENSAJES */}
        {mensaje && (
          <div className="alerta-error">
            {mensaje}
          </div>
        )}

        {/* FORMULARIO */}
        <form onSubmit={handleSubmit}>

          <div className="campo">

            <label htmlFor="usuario">
              Usuario
            </label>

            <input
              id="usuario"
              name="usuario"
              type="text"
              value={usuario}
              onChange={(event) =>
                setUsuario(
                  event.target.value
                )
              }
              autoComplete="username"
              disabled={cargando}
            />

          </div>

          <div className="campo">

            <label htmlFor="password">
              Contraseña
            </label>

            <input
              id="password"
              name="password"
              type="password"
              value={password}
              onChange={(event) =>
                setPassword(
                  event.target.value
                )
              }
              autoComplete="current-password"
              disabled={cargando}
            />

          </div>

          <button
            type="submit"
            disabled={cargando}
          >
            {cargando
              ? "Ingresando..."
              : "Aceptar"}
          </button>

        </form>

      </div>

    </div>
  );
}

export default Login;