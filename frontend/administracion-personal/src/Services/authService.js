const API_URL = import.meta.env.VITE_API_URL;

export async function login(usuario, password) {
  const response = await fetch(
    `${API_URL}/gateway/auth/login`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        usuario,
        password,
      }),
    }
  );

  const data = await response.json();

  if (!response.ok) {
    throw {
      statusCode: response.status,
      message:
        data.message ||
        "Ocurrió un error al iniciar sesión.",
    };
  }

  return data;
}