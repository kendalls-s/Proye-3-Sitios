import {
    useEffect,
    useState,
} from "react";
import { Link } from "react-router";    

import {useAuth} from "../context/AuthContext";
import { obtenerPuestosActivos } from "../Services/puestosService";

import "./Puestos.css";

function Puestos() {
    const { token } = useAuth();
    const [puestos, setPuestos] = useState([]);
    const [cargando, setCargando] =
        useState(true);

    const [error, setError] =
        useState("");

    useEffect(() => {
        let cancelado = false;

        async function cargarPuestos() {
        setCargando(true);
        setError("");

        try {
            const respuesta =
            await obtenerPuestosActivos(token);

            if (!cancelado) {
            setPuestos(respuesta.data);
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

    // Vista pagina
    return (
        <div>

        <div className="page-header">
            <h1>Puestos disponibles</h1>

            <p>
            Seleccione un puesto para ver los oferentes disponibles.
            </p>
        </div>

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
                </tr>
                </thead>

                <tbody>
                {puestos.length === 0 ? (
                    <tr>
                    <td className="sin-datos">
                        No hay puestos disponibles en este momento.
                    </td>
                    </tr>
                ) : (
                    puestos.map((puesto) => (
                    <tr key={puesto.codigo}>
                        <td>
                        <Link
                            className="enlace-puesto"
                            to={`/oferentes/${puesto.codigo}`}
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