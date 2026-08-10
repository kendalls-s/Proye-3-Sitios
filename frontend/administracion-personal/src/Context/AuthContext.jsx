import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";

const AuthContext = createContext(null);

const TIEMPO_INACTIVIDAD = 5 * 60 * 1000;

export function AuthProvider({ children }) {
  const temporizadorRef = useRef(null);

  const [token, setToken] = useState(() => {
    return localStorage.getItem("token");
  });

  const [usuario, setUsuario] = useState(() => {
    const usuarioGuardado =
      localStorage.getItem("usuario");

    return usuarioGuardado
      ? JSON.parse(usuarioGuardado)
      : null;
  });

  // Indica por qué se cerró la sesión.
  const [motivoCierre, setMotivoCierre] =
    useState(null);

  const limpiarSesion = useCallback(() => {
    localStorage.removeItem("token");
    localStorage.removeItem("usuario");
    localStorage.removeItem("ultimaActividad");

    setToken(null);
    setUsuario(null);

    if (temporizadorRef.current) {
      clearTimeout(temporizadorRef.current);
    }
  }, []);

  const iniciarSesion = (
    datosUsuario
  ) => {
    localStorage.setItem(
      "token",
      datosUsuario.token
    );

    localStorage.setItem(
      "usuario",
      JSON.stringify(datosUsuario)
    );

    localStorage.setItem(
      "ultimaActividad",
      Date.now().toString()
    );

    setMotivoCierre(null);

    setToken(datosUsuario.token);
    setUsuario(datosUsuario);
  };

  // Cierre realizado por el usuario.
  const cerrarSesion = useCallback(() => {
    setMotivoCierre("manual");
    limpiarSesion();
  }, [limpiarSesion]);

  // Cierre por 5 minutos de inactividad.
  const expirarSesion = useCallback(() => {
    setMotivoCierre("expirada");
    limpiarSesion();
  }, [limpiarSesion]);

  const limpiarMotivoCierre =
    useCallback(() => {
      setMotivoCierre(null);
    }, []);

  useEffect(() => {
    if (!token) {
      return;
    }

    const iniciarTemporizador = (
      tiempo = TIEMPO_INACTIVIDAD
    ) => {
      if (temporizadorRef.current) {
        clearTimeout(
          temporizadorRef.current
        );
      }

      temporizadorRef.current =
        setTimeout(() => {
          expirarSesion();
        }, tiempo);
    };

    const registrarActividad = () => {
      localStorage.setItem(
        "ultimaActividad",
        Date.now().toString()
      );

      iniciarTemporizador();
    };

    const ultimaActividad = Number(
      localStorage.getItem(
        "ultimaActividad"
      )
    );

    if (ultimaActividad) {
      const tiempoTranscurrido =
        Date.now() - ultimaActividad;

      if (
        tiempoTranscurrido >=
        TIEMPO_INACTIVIDAD
      ) {
        expirarSesion();
        return;
      }

      iniciarTemporizador(
        TIEMPO_INACTIVIDAD -
          tiempoTranscurrido
      );
    } else {
      localStorage.setItem(
        "ultimaActividad",
        Date.now().toString()
      );

      iniciarTemporizador();
    }

    const eventos = [
      "mousemove",
      "mousedown",
      "keydown",
      "scroll",
      "touchstart",
    ];

    eventos.forEach((evento) => {
      window.addEventListener(
        evento,
        registrarActividad
      );
    });

    return () => {
      eventos.forEach((evento) => {
        window.removeEventListener(
          evento,
          registrarActividad
        );
      });

      if (temporizadorRef.current) {
        clearTimeout(
          temporizadorRef.current
        );
      }
    };
  }, [token, expirarSesion]);

  const estaAutenticado = !!token;

  return (
    <AuthContext.Provider
      value={{
        token,
        usuario,
        estaAutenticado,
        motivoCierre,
        iniciarSesion,
        cerrarSesion,
        limpiarMotivoCierre,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}