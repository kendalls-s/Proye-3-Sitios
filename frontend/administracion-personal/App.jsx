import {
  BrowserRouter,
  Navigate,
  Route,
  Routes,
} from "react-router";

import Login from "./pages/Login";
import Bienvenida from "./pages/Bienvenida";
import Puestos from "./pages/Puestos";
import DetalleOferente from "./pages/DetalleOferente";

import MainLayout from "./layouts/MainLayout";

import {
  AuthProvider
} from "./context/AuthContext";

import ProtectedRoute
  from "./routes/ProtectedRoute";


function App() {
  return (
    <BrowserRouter>

      <AuthProvider>

        <Routes>

          {/* RUTA PRINCIPAL */}
          <Route
            path="/"
            element={
              <Navigate
                to="/login"
                replace
              />
            }
          />


          {/* LOGIN */}
          <Route
            path="/login"
            element={<Login />}
          />


          {/* ======================= */}
          {/* ZONA AUTENTICADA         */}
          {/* ======================= */}

          <Route
            element={
              <ProtectedRoute>
                <MainLayout />
              </ProtectedRoute>
            }
          >

            <Route
              path="/bienvenida"
              element={<Bienvenida />}
            />

            <Route
              path="/puestos"
              element={<Puestos />}
            />

            <Route
              path="/oferentes/:identificacion/contratar/:codigoPuesto"
              element={<DetalleOferente />}
            />

          </Route>


          


          {/* CUALQUIER URL INVÁLIDA */}
          <Route
            path="*"
            element={
              <Navigate
                to="/login"
                replace
              />
            }
          />

        </Routes>

      </AuthProvider>

    </BrowserRouter>
  );
}

export default App;