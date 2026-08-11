import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router";
import { useAuth } from "../context/AuthContext";
import { obtenerPuestosActivos } from "../Services/puestosService";
import "./Puestos.css";

function Puestos() {
  const { token } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [puestos, setPuestos] = useState([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");

  const mensajeExito = location.state?.mensajeExito || "";

  useEffect(() => {
    let cancelado = false;

    async function cargarPuestos() {
      setCargando(true);
      setError("");
      try {
        const respuesta = await obtenerPuestosActivos(token);
        if (!cancelado) setPuestos(respuesta.data || []);
      } catch (err) {
        if (!cancelado) {
          setError(err?.message || "No fue posible obtener el listado de puestos.");
        }
      } finally {
        if (!cancelado) setCargando(false);
      }
    }

    cargarPuestos();
    return () => { cancelado = true; };
  }, [token]);

  useEffect(() => {
    if (!mensajeExito) return undefined;

    const temporizador = window.setTimeout(() => {
      navigate(location.pathname, { replace: true, state: {} });
    }, 3500);

    return () => window.clearTimeout(temporizador);
  }, [mensajeExito, navigate, location.pathname]);

  return (
    <div>
      <div className="page-header">
        <h1>Puestos disponibles</h1>
        <p>Seleccione un puesto para ver los oferentes que se postularon a él.</p>
      </div>

      {mensajeExito && <div className="alerta-exito">{mensajeExito}</div>}
      {error && <div className="alerta-error">{error}</div>}

      <div className="welcome-card">
        {cargando ? (
          <p>Cargando puestos...</p>
        ) : (
          <table className="tabla-puestos">
            <thead>
              <tr>
                <th>Código</th>
                <th>Puesto</th>
              </tr>
            </thead>
            <tbody>
              {puestos.length === 0 ? (
                <tr>
                  <td className="sin-datos" colSpan="2">
                    No hay puestos disponibles en este momento.
                  </td>
                </tr>
              ) : (
                puestos.map((puesto) => (
                  <tr key={puesto.codigo}>
                    <td>{puesto.codigo}</td>
                    <td>
                      {/* El nombre del puesto es un enlace hacia Core7
                          (listado de oferentes aptos para ese puesto). */}
                      <Link
                        className="enlace-puesto"
                        to={`/puestos/${encodeURIComponent(puesto.codigo)}/oferentes`}
                      >
                        {puesto.nombre}
                      </Link>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default Puestos;
