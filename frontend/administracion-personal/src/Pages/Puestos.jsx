import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router";
import { useAuth } from "../Context/AuthContext";
import { obtenerPuestosActivos, obtenerOferentesPorPuesto } from "../Services/puestosService";

import "./Puestos.css";

function Puestos() {
    const { token } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();
    const mensajeExito = location.state?.mensajeExito || "";
    const [puestos, setPuestos] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        let cancelado = false;

        async function cargarPuestos() {
            setCargando(true);
            setError("");

            try {
                const respuesta = await obtenerPuestosActivos(token);
                const listaPuestos = respuesta?.data || [];

                const puestosConOferentes = await Promise.all(
                    listaPuestos.map(async (puesto) => {
                        try {
                            const respuestaOferentes = await obtenerOferentesPorPuesto(
                                token,
                                puesto.codigo
                            );

                            return {
                                ...puesto,
                                oferentes: respuestaOferentes?.data || [],
                            };
                        } catch {
                            return {
                                ...puesto,
                                oferentes: [],
                            };
                        }
                    })
                );

                if (!cancelado) {
                    setPuestos(puestosConOferentes);
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
            navigate(location.pathname, { replace: true, state: {} });
        }, 3500);

        return () => window.clearTimeout(temporizador);
    }, [mensajeExito, navigate, location.pathname]);

    return (
        <div>
            <div className="page-header">
                <h1>Puestos disponibles</h1>

                <p>
                    Seleccione el nombre del oferente para consultar su detalle y
                    convertirlo en empleado.
                </p>
            </div>

            {mensajeExito && (
                <div className="alerta-exito">{mensajeExito}</div>
            )}

            <div className="welcome-card">
                {error && (
                    <div className="alerta-error">
                        {error}
                    </div>
                )}

                {cargando ? (
                    <p>Cargando puestos...</p>
                ) : (
                    <table className="tabla-puestos">
                        <thead>
                            <tr>
                                <th>Puesto</th>
                                <th>Oferente</th>
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
                                puestos.flatMap((puesto) => {
                                    if (!puesto.oferentes?.length) {
                                        return [
                                            <tr key={`${puesto.codigo}-sin-oferente`}>
                                                <td>{puesto.nombre}</td>
                                                <td className="sin-datos">
                                                    No hay oferentes disponibles.
                                                </td>
                                            </tr>,
                                        ];
                                    }

                                    return puesto.oferentes.map((oferente) => (
                                        <tr key={`${puesto.codigo}-${oferente.idOferente}`}>
                                            <td>{puesto.nombre}</td>
                                            <td>
                                                <Link
                                                    className="enlace-puesto"
                                                    to={`/oferentes/${encodeURIComponent(
                                                        oferente.identificacion
                                                    )}/contratar/${encodeURIComponent(
                                                        puesto.codigo
                                                    )}`}
                                                >
                                                    {oferente.nombreCompleto}
                                                </Link>
                                            </td>
                                        </tr>
                                    ));
                                })
                            )}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
}

export default Puestos;
