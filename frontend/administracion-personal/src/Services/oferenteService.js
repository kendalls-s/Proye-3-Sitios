const API_URL = import.meta.env.VITE_API_URL;

async function requestJson(url, options = {}) {
  let response;

  try {
    response = await fetch(url, {
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
      },
      ...options,
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
  const data = await requestJson(`${API_URL}/gateway/core3/empleados`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ identificacion, codigoPuesto }),
  });

  return data?.data ?? data;
}
