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
      "No se pudo contactar al servicio. Verifique que el gateway y los microservicios estén disponibles."
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

/**
 * Core7 consume el microservicio Core2 (oferentes aptos) SIEMPRE a través del
 * gateway. El flujo es:
 *
 *   React  ->  GET {gateway}/gateway/core2/oferentes-aptos/{codigoPuesto}
 *          ->  (YARP quita el prefijo /gateway/core2)
 *          ->  SRV_Core2_OferentesAptos  GET /oferentes-aptos/{codigoPuesto}
 *
 * El microservicio responde con el envelope
 * { success, statusCode, message, data } y aquí devolvemos solo `data`
 * (arreglo de { idOferente, identificacion, nombreCompleto }).
 */
export async function obtenerOferentesAptos(token, codigoPuesto) {
  const data = await requestJson(
    `${API_URL}/gateway/core2/oferentes-aptos/${encodeURIComponent(codigoPuesto)}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return data?.data ?? data;
}
