import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { useAuth } from "../context/AuthContext";
import { crearEmpleado, obtenerDetalleOferente } from "../Services/oferenteService";
import "./DetalleOferente.css";

/**
 * Core9 - "Yo como usuario del sistema quiero una pantalla de detalle de
 * oferente para ver su información y convertirlo en empleado".
 *
 * Criterios de aceptación cubiertos:
 *  - Como usuario autenticado se ve el detalle del oferente seleccionado en
 *    Core8 (la ruta está protegida por ProtectedRoute).
 *  - En el detalle se muestra TODA la información que el oferente registró
 *    previamente: datos personales, contacto, preparación académica,
 *    experiencia laboral, currículums y postulaciones.
 *  - Botón "Crear empleado": toma la información registrada como oferente y la
 *    registra en las tablas de empleado (usa el servicio de Core3).
 *  - Botón "Cancelar": regresa a la pantalla de listado de oferentes (Core7).
 *
 * Aspectos técnicos:
 *  - Usa el servicio de Core8 para obtener el detalle (obtenerDetalleOferente).
 *  - Usa el servicio de Core3 para registrar el empleado (crearEmpleado).
 *  - Todo el consumo se hace SIEMPRE a través del gateway:
 *      React -> {gateway}/gateway/core8/... -> SRV_Core8_DetalleOferente
 *      React -> {gateway}/gateway/core3/... -> SRV_Core3_CreacionEmpleados
 */

function formatearFecha(fecha) {
  if (!fecha) return "—";
  const valor = new Date(fecha);
  if (Number.isNaN(valor.getTime())) return fecha;
  return valor.toLocaleDateString("es-CR");
}

function formatearTamano(bytes) {
  if (bytes === null || bytes === undefined || Number.isNaN(Number(bytes))) {
    return "—";
  }
  const valor = Number(bytes);
  if (valor < 1024) return `${valor} B`;
  if (valor < 1024 * 1024) return `${(valor / 1024).toFixed(1)} KB`;
  return `${(valor / (1024 * 1024)).toFixed(1)} MB`;
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

  // Ruta de la pantalla previa (Core7 - listado de oferentes aptos del puesto).
  const rutaListadoOferentes = `/puestos/${encodeURIComponent(
    codigoPuesto
  )}/oferentes`;

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

  async function handleCrearEmpleado() {
    setContratando(true);
    setErrorContratacion("");

    try {
      // Core3: registra el empleado a partir del oferente y el puesto.
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

  function handleCancelar() {
    // Regresa a la pantalla previa (Core7 - listado de oferentes del puesto).
    navigate(rutaListadoOferentes);
  }

  return (
    <div>
      <div className="page-header">
        <Link className="detalle-volver" to={rutaListadoOferentes}>
          ← Volver al listado de oferentes
        </Link>
        <h1>Detalle de oferente y contratación</h1>
        <p>
          Identificación: <strong>{identificacion}</strong> · Puesto:{" "}
          <strong>{codigoPuesto}</strong>
        </p>
      </div>

      {cargando && (
        <div className="welcome-card">Cargando detalle del oferente...</div>
      )}

      {errorCarga && <div className="alerta-error">{errorCarga}</div>}

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
                {detalle.tipoIdentificacion
                  ? ` (${detalle.tipoIdentificacion})`
                  : ""}
              </dd>
              <dt>Fecha de nacimiento</dt>
              <dd>{formatearFecha(detalle.fechaNacimiento)}</dd>
              <dt>Dirección</dt>
              <dd>{detalle.direccion || "—"}</dd>
              <dt>Ubicación</dt>
              <dd>
                {[
                  detalle.nombreDistrito,
                  detalle.nombreCanton,
                  detalle.nombreProvincia,
                ]
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
                    {detalle.correos.map((correo) => (
                      <li key={correo}>{correo}</li>
                    ))}
                  </ul>
                ) : (
                  <p>Sin correos registrados.</p>
                )}
              </div>
              <div>
                <h3>Teléfonos</h3>
                {detalle.telefonos?.length ? (
                  <ul>
                    {detalle.telefonos.map((telefono) => (
                      <li key={telefono}>{telefono}</li>
                    ))}
                  </ul>
                ) : (
                  <p>Sin teléfonos registrados.</p>
                )}
              </div>
            </div>
          </section>

          <section className="welcome-card">
            <h2>Preparación académica</h2>
            {detalle.preparacionAcademica?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead>
                    <tr>
                      <th>Institución</th>
                      <th>Título</th>
                      <th>Inicio</th>
                      <th>Fin</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.preparacionAcademica.map((item, i) => (
                      <tr key={i}>
                        <td>{item.institucion}</td>
                        <td>{item.titulo}</td>
                        <td>{formatearFecha(item.fechaInicio)}</td>
                        <td>{formatearFecha(item.fechaFin)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p>Sin preparación académica registrada.</p>
            )}
          </section>

          <section className="welcome-card">
            <h2>Experiencia laboral</h2>
            {detalle.experienciaLaboral?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead>
                    <tr>
                      <th>Empresa</th>
                      <th>Puesto</th>
                      <th>Inicio</th>
                      <th>Fin</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.experienciaLaboral.map((item, i) => (
                      <tr key={i}>
                        <td>{item.empresa}</td>
                        <td>{item.puesto}</td>
                        <td>{formatearFecha(item.fechaInicio)}</td>
                        <td>{formatearFecha(item.fechaFin)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p>Sin experiencia laboral registrada.</p>
            )}
          </section>

          <section className="welcome-card">
            <h2>Currículums</h2>
            {detalle.curriculums?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead>
                    <tr>
                      <th>Archivo</th>
                      <th>Tipo</th>
                      <th>Tamaño</th>
                      <th>Fecha de carga</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.curriculums.map((item, i) => (
                      <tr key={i}>
                        <td>{item.nombreArchivo}</td>
                        <td>{item.tipoArchivo || "—"}</td>
                        <td>{formatearTamano(item.tamanoBytes)}</td>
                        <td>{formatearFecha(item.fechaCarga)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p>Sin currículums registrados.</p>
            )}
          </section>

          <section className="welcome-card">
            <h2>Postulaciones</h2>
            {detalle.postulaciones?.length ? (
              <div className="tabla-responsive">
                <table className="tabla-detalle">
                  <thead>
                    <tr>
                      <th>Puesto</th>
                      <th>Fecha</th>
                      <th>Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.postulaciones.map((item, i) => (
                      <tr key={i}>
                        <td>
                          {item.codigoPuesto} - {item.nombrePuesto}
                        </td>
                        <td>{formatearFecha(item.fechaPostulacion)}</td>
                        <td>{item.estado}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p>Sin postulaciones registradas.</p>
            )}
          </section>

          <section className="welcome-card contratacion-card">
            <h2>Contratación</h2>
            <p>
              El oferente será convertido en empleado para el puesto{" "}
              <strong>{codigoPuesto}</strong>. Se copiará su información a las
              estructuras de empleado y se generará su acción de personal.
            </p>

            {errorContratacion && (
              <div className="alerta-error">{errorContratacion}</div>
            )}

            <div className="contratacion-acciones">
              <button
                className="btn-contratar"
                type="button"
                onClick={handleCrearEmpleado}
                disabled={contratando}
              >
                {contratando ? "Creando empleado..." : "Crear empleado"}
              </button>

              <button
                className="btn-cancelar"
                type="button"
                onClick={handleCancelar}
                disabled={contratando}
              >
                Cancelar
              </button>
            </div>
          </section>
        </div>
      )}
    </div>
  );
}

export default DetalleOferente;
