import { createContext, useContext, useState, useEffect } from 'react';
import { authService, type User } from '../services/authService';
import { apiClient } from '../services/api';

// 1. Create the Auth Context
const AuthContext = createContext<{
  user: User | null;
  token: string | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isLoading: boolean;
} | undefined>(undefined);

// 2. Create the Auth Provider Component
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadUser() {
      if (token) {
        try {
          apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
          const currentUser = await authService.getMe();
          setUser(currentUser);
        } catch (error) {
          console.error('Failed to fetch user', error);
          // Token might be invalid, so clear it
          localStorage.removeItem('token');
          setToken(null);
          delete apiClient.defaults.headers.common['Authorization'];
        }
      }
      setLoading(false);
    }

    loadUser();
  }, [token]);

  const login = async (email: string, password: string) => {
    const { token: newToken, user: newUser } = await authService.login(email, password);
    localStorage.setItem('token', newToken);
    setToken(newToken);
    setUser(newUser);
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${newToken}`;
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
    delete apiClient.defaults.headers.common['Authorization'];
  };

  const value = {
    user,
    token,
    login,
    logout,
    isAuthenticated: !!token,
    isLoading: loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// 3. Create the useAuth Hook
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
