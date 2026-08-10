import { Navigate } from "react-router";
import { useAuth } from "../context/AuthContext";

function ProtectedRoute({ children }) {
  const {
    estaAutenticado,
    motivoCierre,
  } = useAuth();

  if (estaAutenticado) {
    return children;
  }

  // Cierre manual:
  // regresar al login SIN mensaje.
  if (motivoCierre === "manual") {
    return (
      <Navigate
        to="/login"
        replace
      />
    );
  }

  // Sesión expirada:
  // regresar al login con el mensaje correcto.
  if (motivoCierre === "expirada") {
    return (
      <Navigate
        to="/login"
        replace
        state={{
          mensaje:
            "La sesión ha expirado.",
        }}
      />
    );
  }

  // Intentó ingresar sin autenticarse.
  return (
    <Navigate
      to="/login"
      replace
      state={{
        mensaje:
          "Por favor inicie sesión para utilizar el sistema",
      }}
    />
  );
}

export default ProtectedRoute;