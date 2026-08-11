const API_URL = import.meta.env.VITE_API_URL;

export async function obtenerPuestosActivos(token) {
  const response = await fetch(
    `${API_URL}/gateway/core1/puestos-activos`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    }
  );

  const data = await response.json();

  if (!response.ok) {
    throw {
      statusCode: response.status,
      message:
        data.message ||
        "No fue posible obtener el listado de puestos.",
    };
  }

  return data;
}
/**
 * Obtiene los oferentes aptos para un puesto utilizando Core2.
 * Core2 expone GET /oferentes-aptos/{codigoPuesto}.
 */
export async function obtenerOferentesPorPuesto(token, codigoPuesto) {
  const response = await fetch(
    `${API_URL}/gateway/core2/oferentes-aptos/${encodeURIComponent(codigoPuesto)}`,
    {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    }
  );

  const data = await response.json();

  if (!response.ok) {
    throw {
      statusCode: response.status,
      message:
        data?.message ||
        "No fue posible obtener los oferentes del puesto.",
    };
  }

  return data;
}
