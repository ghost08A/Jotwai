import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ROUTES } from "../../shared/routes/paths";
import { Home } from "../pages/Home";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path={ROUTES.home} element={<Home />} />
      </Routes>
    </BrowserRouter>
  );
}
