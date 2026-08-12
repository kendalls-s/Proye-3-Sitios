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

  // Paginación
  const [paginaActual, setPaginaActual] = useState(1);
  const elementosPorPagina = 10;

  const mensajeExito = location.state?.mensajeExito || "";

  useEffect(() => {
    let cancelado = false;

    async function cargarPuestos() {
      setCargando(true);
      setError("");

      try {
        const respuesta = await obtenerPuestosActivos(token);

        if (!cancelado) {
          setPuestos(respuesta.data || []);
          setPaginaActual(1);
        }
      } catch (err) {
        if (!cancelado) {
          setError(
            err?.message ||
              "No fue posible obtener el listado de puestos."
          );
        }
      } finally {
        if (!cancelado) {
          setCargando(false);
        }
      }
    }

    cargarPuestos();

    return () => {
      cancelado = true;
    };
  }, [token]);

  useEffect(() => {
    if (!mensajeExito) return undefined;

    const temporizador = window.setTimeout(() => {
      navigate(location.pathname, {
        replace: true,
        state: {},
      });
    }, 3500);

    return () => window.clearTimeout(temporizador);
  }, [mensajeExito, navigate, location.pathname]);

  // ==========================================
  // PAGINACIÓN
  // ==========================================

  const totalPaginas = Math.ceil(
    puestos.length / elementosPorPagina
  );

  const indiceUltimo = paginaActual * elementosPorPagina;
  const indicePrimero = indiceUltimo - elementosPorPagina;

  const puestosPagina = puestos.slice(
    indicePrimero,
    indiceUltimo
  );

  const irPaginaAnterior = () => {
    if (paginaActual > 1) {
      setPaginaActual((pagina) => pagina - 1);
    }
  };

  const irPaginaSiguiente = () => {
    if (paginaActual < totalPaginas) {
      setPaginaActual((pagina) => pagina + 1);
    }
  };

  const cambiarPagina = (pagina) => {
    setPaginaActual(pagina);
  };

  return (
    <div>
      <div className="page-header">
        <h1>Puestos disponibles</h1>
        <p>
          Seleccione un puesto para ver los oferentes que se
          postularon a él.
        </p>
      </div>

      {mensajeExito && (
        <div className="alerta-exito">
          {mensajeExito}
        </div>
      )}

      {error && (
        <div className="alerta-error">
          {error}
        </div>
      )}

      <div className="welcome-card">
        {cargando ? (
          <p>Cargando puestos...</p>
        ) : (
          <>
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
                  puestosPagina.map((puesto) => (
                    <tr key={puesto.codigo}>
                      <td>{puesto.codigo}</td>

                      <td>
                        <Link
                          className="enlace-puesto"
                          to={`/puestos/${encodeURIComponent(
                            puesto.codigo
                          )}/oferentes`}
                        >
                          {puesto.nombre}
                        </Link>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            {puestos.length > 0 && (
              <div className="contenedor-paginacion">
                <div className="info-paginacion">
                  Mostrando{" "}
                  {indicePrimero + 1} -{" "}
                  {Math.min(
                    indiceUltimo,
                    puestos.length
                  )}{" "}
                  de {puestos.length} puestos
                </div>

                {totalPaginas > 1 && (
                  <div className="paginacion">
                    <button
                      type="button"
                      className="boton-paginacion"
                      onClick={irPaginaAnterior}
                      disabled={paginaActual === 1}
                    >
                      Anterior
                    </button>

                    {Array.from(
                      { length: totalPaginas },
                      (_, indice) => indice + 1
                    ).map((pagina) => (
                      <button
                        type="button"
                        key={pagina}
                        className={`boton-pagina ${
                          paginaActual === pagina
                            ? "pagina-activa"
                            : ""
                        }`}
                        onClick={() =>
                          cambiarPagina(pagina)
                        }
                      >
                        {pagina}
                      </button>
                    ))}

                    <button
                      type="button"
                      className="boton-paginacion"
                      onClick={irPaginaSiguiente}
                      disabled={
                        paginaActual === totalPaginas
                      }
                    >
                      Siguiente
                    </button>
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default Puestos;