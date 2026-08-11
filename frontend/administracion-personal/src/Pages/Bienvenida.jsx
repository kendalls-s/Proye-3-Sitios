import { useAuth } from "../Context/AuthContext";

function Bienvenida() {
  const { usuario } = useAuth();

  return (
    <div>

      <div className="page-header">
        <h1>Bienvenida</h1>

        <p>
          Sistema de Administración de Personal
        </p>
      </div>


      <div className="welcome-card">

        <h2>
          Bienvenido,{" "}
          {usuario?.nombreCompleto ||
            usuario?.usuario ||
            "Usuario"}
        </h2>

        <p>
          Has iniciado sesión correctamente
          en el sistema de Administración
          de Personal.
        </p>

      </div>

    </div>
  );
}

export default Bienvenida;