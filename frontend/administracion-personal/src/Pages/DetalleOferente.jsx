import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { useAuth } from "../context/AuthContext";
import { crearEmpleado, obtenerDetalleOferente } from "../Services/oferenteService";
import "./DetalleOferente.css";

function formatearFecha(fecha) {
  if (!fecha) return "—";
  const valor = new Date(fecha);
  if (Number.isNaN(valor.getTime())) return fecha;
  return valor.toLocaleDateString("es-CR");
}

function DetalleOferente() {
  const { token } = useAuth();
  const { identificacion, codigoPuesto } = useParams();
  const navigate = useNavigate();

  const [detalle, setDetalle] = useState(null);
  const [cargando, setCargando] = useState(true);
  const [errorCarga, setErrorCarga] = useState("");
  const [contratando, setContratando] = useState(false);
  const [errorContratacion, setErrorContratacion] = useState("");

  useEffect(() => {
    let cancelado = false;

    async function cargarDetalle() {
      setCargando(true);
      setErrorCarga("");

      try {
        const data = await obtenerDetalleOferente(token, identificacion);
        if (!cancelado) setDetalle(data);
      } catch (err) {
        if (!cancelado) {
          setErrorCarga(
            err?.message || "No fue posible obtener el detalle del oferente."
          );
        }
      } finally {
        if (!cancelado) setCargando(false);
      }
    }

    cargarDetalle();

    return () => {
      cancelado = true;
    };
  }, [token, identificacion]);

  async function handleContratar() {
    setContratando(true);
    setErrorContratacion("");

    try {
      await crearEmpleado(token, identificacion, codigoPuesto);
      navigate("/puestos", {
        state: { mensajeExito: "Empleado creado con éxito" },
      });
    } catch (err) {
      setErrorContratacion(
        err?.message || "No fue posible crear el empleado."
      );
    } finally {
      setContratando(false);
    }
  }

  return (
    <div>
      <div className="page-header">
        <Link className="detalle-volver" to="/puestos">
          ← Volver a puestos
        </Link>
        <h1>Detalle de oferente y contratación</h1>
        <p>
          Identificación: <strong>{identificacion}</strong> · Puesto: {" "}
          <strong>{codigoPuesto}</strong>
        </p>
      </div>

      {cargando && <div className="welcome-card">Cargando detalle del oferente...</div>}

      {errorCarga && (
        <div className="alerta-error">{errorCarga}</div>
      )}

      {!cargando && !errorCarga && detalle && (
        <div className="detalle-contenedor">
          <section className="welcome-card">
            <h2>Datos personales</h2>
            <dl className="detalle-datos">
              <dt>Nombre completo</dt>
              <dd>{detalle.nombreCompleto || "—"}</dd>
              <dt>Identificación</dt>
              <dd>
                {detalle.identificacion || identificacion}
                {detalle.tipoIdentificacion ? ` (${detalle.tipoIdentificacion})` : ""}
              </dd>
              <dt>Fecha de nacimiento</dt>
              <dd>{formatearFecha(detalle.fechaNacimiento)}</dd>
              <dt>Dirección</dt>
              <dd>{detalle.direccion || "—"}</dd>
              <dt>Ubicación</dt>
              <dd>
                {[detalle.nombreDistrito, detalle.nombreCanton, detalle.nombreProvincia]
                  .filter(Boolean)
                  .join(", ") || "—"}
              </dd>
              <dt>Fecha de registro</dt>
              <dd>{formatearFecha(detalle.fechaRegistro)}</dd>
            </dl>
          </section>

          <section className="welcome-card">
            <h2>Contacto</h2>
            <div className="detalle-columnas">
              <div>
                <h3>Correos</h3>
                {detalle.correos?.length ? (
                  <ul>
                    {detalle.correos.map((correo) => <li key={correo}>{correo}</li>)}
                  </ul>
                ) : <p>Sin correos registrados.</p>}
              </div>
              <div>
                <h3>Teléfonos</h3>
                {detalle.telefonos?.length ? (
                  <ul>
                    {detalle.telefonos.map((telefono) => <li key={telefono}>{telefono}</li>)}
                  </ul>
                ) : <p>Sin teléfonos registrados.</p>}
              </div>
            </div>
          </section>

          <section className="welcome-card">
            <h2>Preparación académica</h2>
            {detalle.preparacionAcademica?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead><tr><th>Institución</th><th>Título</th><th>Inicio</th><th>Fin</th></tr></thead>
                  <tbody>
                    {detalle.preparacionAcademica.map((item, i) => (
                      <tr key={i}>
                        <td>{item.institucion}</td><td>{item.titulo}</td>
                        <td>{formatearFecha(item.fechaInicio)}</td><td>{formatearFecha(item.fechaFin)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : <p>Sin preparación académica registrada.</p>}
          </section>

          <section className="welcome-card">
            <h2>Experiencia laboral</h2>
            {detalle.experienciaLaboral?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead><tr><th>Empresa</th><th>Puesto</th><th>Inicio</th><th>Fin</th></tr></thead>
                  <tbody>
                    {detalle.experienciaLaboral.map((item, i) => (
                      <tr key={i}>
                        <td>{item.empresa}</td><td>{item.puesto}</td>
                        <td>{formatearFecha(item.fechaInicio)}</td><td>{formatearFecha(item.fechaFin)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : <p>Sin experiencia laboral registrada.</p>}
          </section>

          <section className="welcome-card">
            <h2>Postulaciones</h2>
            {detalle.postulaciones?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead><tr><th>Puesto</th><th>Fecha</th><th>Estado</th></tr></thead>
                  <tbody>
                    {detalle.postulaciones.map((item, i) => (
                      <tr key={i}>
                        <td>{item.codigoPuesto} - {item.nombrePuesto}</td>
                        <td>{formatearFecha(item.fechaPostulacion)}</td><td>{item.estado}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : <p>Sin postulaciones registradas.</p>}
          </section>

          <section className="welcome-card contratacion-card">
            <h2>Contratación</h2>
            <p>El oferente será convertido en empleado para el puesto seleccionado.</p>
            {errorContratacion && (
              <div className="alerta-error">{errorContratacion}</div>
            )}
            <button
              className="btn-contratar"
              type="button"
              onClick={handleContratar}
              disabled={contratando}
            >
              {contratando ? "Creando empleado..." : "Crear empleado"}
            </button>
          </section>
        </div>
      )}
    </div>
  );
}

export default DetalleOferente;
