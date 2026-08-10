import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router";
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
  const [identificacionPorPuesto, setIdentificacionPorPuesto] = useState({});

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

  function irAContratar(codigoPuesto) {
    const identificacion = (identificacionPorPuesto[codigoPuesto] || "").trim();
    if (!identificacion) return;
    navigate(
      `/oferentes/${encodeURIComponent(identificacion)}/contratar/${encodeURIComponent(codigoPuesto)}`
    );
  }

  return (
    <div>
      <div className="page-header">
        <h1>Puestos disponibles</h1>
        <p>Seleccione un puesto para consultar un oferente y realizar la contratación.</p>
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
                <th>Puesto</th>
                <th>Identificación del oferente</th>
                <th>Acción</th>
              </tr>
            </thead>
            <tbody>
              {puestos.length === 0 ? (
                <tr>
                  <td className="sin-datos" colSpan="3">
                    No hay puestos disponibles en este momento.
                  </td>
                </tr>
              ) : (
                puestos.map((puesto) => (
                  <tr key={puesto.codigo}>
                    <td>{puesto.nombre}</td>
                    <td>
                      <input
                        className="input-identificacion"
                        type="text"
                        placeholder="Identificación"
                        value={identificacionPorPuesto[puesto.codigo] || ""}
                        onChange={(e) =>
                          setIdentificacionPorPuesto((prev) => ({
                            ...prev,
                            [puesto.codigo]: e.target.value,
                          }))
                        }
                      />
                    </td>
                    <td>
                      <button
                        className="btn-detalle"
                        type="button"
                        onClick={() => irAContratar(puesto.codigo)}
                        disabled={!(identificacionPorPuesto[puesto.codigo] || "").trim()}
                      >
                        Ver detalle y contratar
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        )}
      </div>

      <p className="nota-core9">
        Core9 consulta el detalle mediante Core8 y realiza la contratación mediante Core3.
      </p>
    </div>
  );
}

export default Puestos;
