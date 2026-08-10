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