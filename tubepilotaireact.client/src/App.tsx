import { Routes, Route, Outlet } from 'react-router-dom';
import { Sidebar } from './components/Sidebar';
import { TopNav } from './components/TopNav';
import Dashboard from './pages/Dashboard';
import { PromptCategoriesPage } from './pages/PromptCategoriesPage';
import { PromptsPage } from './pages/PromptsPage';
import { PromptVariablesPage } from './pages/PromptVariablesPage';
import { LoginPage } from './pages/LoginPage';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { useAuth } from './contexts/AuthContext';

function App() {
  const { isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-screen">
        <div className="text-xl">Loading...</div>
      </div>
    );
  }

  const AppLayout = () => (
    <div className="flex h-screen bg-gray-50 dark:bg-gray-950">
      <Sidebar />
      <div className="flex-1 flex flex-col overflow-hidden">
        <TopNav />
        <main className="flex-1 overflow-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<Dashboard />} />
          <Route path="/prompt-categories" element={<PromptCategoriesPage />} />
          <Route path="/prompts" element={<PromptsPage />} />
          <Route path="/prompt-variables" element={<PromptVariablesPage />} />
        </Route>
      </Route>
    </Routes>
  );
}

export default App;

