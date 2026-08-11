import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { useAuth } from "../context/AuthContext";
import { obtenerOferentesAptos } from "../Services/oferentesAptosService";
import "./ListadoOferentes.css";

/**
 * Core7 - "Yo como usuario del sistema quiero una pantalla de listado de
 * oferentes para seleccionar el que será el nuevo empleado".
 *
 * - Muestra únicamente el nombre completo y la identificación del oferente.
 * - El nombre es un enlace que dirige al detalle del oferente (Core9).
 * - Incluye un botón "Regresar" que vuelve al listado de puestos (Core6).
 * - Los datos provienen del microservicio Core2 (oferentes aptos), consumido
 *   siempre a través del gateway.
 */
function ListadoOferentes() {
  const { token } = useAuth();
  const { codigoPuesto } = useParams();
  const navigate = useNavigate();

  const [oferentes, setOferentes] = useState([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelado = false;

    async function cargarOferentes() {
      setCargando(true);
      setError("");
      try {
        const data = await obtenerOferentesAptos(token, codigoPuesto);
        if (!cancelado) setOferentes(Array.isArray(data) ? data : []);
      } catch (err) {
        if (!cancelado) {
          setError(
            err?.message || "No fue posible obtener el listado de oferentes."
          );
        }
      } finally {
        if (!cancelado) setCargando(false);
      }
    }

    cargarOferentes();
    return () => {
      cancelado = true;
    };
  }, [token, codigoPuesto]);

  return (
    <div>
      <div className="page-header">
        <h1>Oferentes aptos</h1>
        <p>
          Oferentes que cumplen los requisitos del puesto{" "}
          <strong>{codigoPuesto}</strong>. Seleccione un nombre para ver el
          detalle y crear el empleado.
        </p>
      </div>

      {error && <div className="alerta-error">{error}</div>}

      <div className="welcome-card">
        {cargando ? (
          <p>Cargando oferentes...</p>
        ) : (
          <table className="tabla-oferentes">
            <thead>
              <tr>
                <th>Nombre completo</th>
                <th>Identificación</th>
              </tr>
            </thead>
            <tbody>
              {oferentes.length === 0 ? (
                <tr>
                  <td className="sin-datos" colSpan="2">
                    No hay oferentes que cumplan los requisitos de este puesto.
                  </td>
                </tr>
              ) : (
                oferentes.map((oferente) => (
                  <tr key={oferente.identificacion}>
                    <td>
                      <Link
                        className="enlace-oferente"
                        to={`/oferentes/${encodeURIComponent(
                          oferente.identificacion
                        )}/contratar/${encodeURIComponent(codigoPuesto)}`}
                      >
                        {oferente.nombreCompleto}
                      </Link>
                    </td>
                    <td>{oferente.identificacion}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>

      <div className="acciones-listado">
        <button
          className="btn-regresar"
          type="button"
          onClick={() => navigate("/puestos")}
        >
          Regresar
        </button>
      </div>
    </div>
  );
}

export default ListadoOferentes;
