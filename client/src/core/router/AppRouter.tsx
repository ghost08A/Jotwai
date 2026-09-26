import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ROUTES } from "../../shared/routes/paths";
import { PrivateLayout } from "../layouts/PrivateLayout";
import { NotesPage } from "../../modules/notes/components/NotesPage";
import { TrashPage } from "../../modules/notes/components/TrashPage";
import { TopicsPage } from "../../modules/organization/components/TopicsPage";
import { CategoriesPage } from "../../modules/organization/components/CategoriesPage";
import { SearchPage } from "../../modules/search/components/SearchPage";
import { LoginPage } from "../../modules/identity/components/LoginPage";
import { RegisterPage } from "../../modules/identity/components/RegisterPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { PublicOnlyRoute } from "./PublicOnlyRoute";

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<PublicOnlyRoute />}>
          <Route path={ROUTES.login} element={<LoginPage />} />
          <Route path={ROUTES.register} element={<RegisterPage />} />
        </Route>
        <Route element={<ProtectedRoute />}>
          <Route element={<PrivateLayout />}>
            <Route path={ROUTES.home} element={<NotesPage />} />
            <Route path={ROUTES.trash} element={<TrashPage />} />
            <Route path={ROUTES.topics} element={<TopicsPage />} />
            <Route path={ROUTES.categories} element={<CategoriesPage />} />
            <Route path={ROUTES.search} element={<SearchPage />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
