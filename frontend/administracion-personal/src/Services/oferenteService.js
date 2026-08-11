const API_URL = import.meta.env.VITE_API_URL;

async function requestJson(url, options = {}) {
  let response;

  try {
    response = await fetch(url, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
      },
    });
  } catch {
    const error = new Error(
      "No se pudo contactar al servicio. Verifique que los servicios estén disponibles."
    );
    error.statusCode = 0;
    throw error;
  }

  let data = null;
  try {
    data = await response.json();
  } catch {
    data = null;
  }

  if (!response.ok) {
    const error = new Error(
      data?.message ||
      data?.title ||
      data?.detail ||
      `Error inesperado (HTTP ${response.status}).`
    );
    error.statusCode = response.status;
    throw error;
  }

  return data;
}

export async function obtenerDetalleOferente(token, identificacion) {
  const data = await requestJson(
    `${API_URL}/gateway/core8/detalle-oferente/${encodeURIComponent(identificacion)}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return data?.data ?? data;
}

export async function crearEmpleado(token, identificacion, codigoPuesto) {
  // El backend requiere fechaIngreso; se usa la fecha actual (YYYY-MM-DD).
  const fechaIngreso = new Date().toISOString().slice(0, 10);

  const data = await requestJson(`${API_URL}/gateway/core3/empleados`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ identificacion, codigoPuesto, fechaIngreso }),
  });

  return data?.data ?? data;
}
