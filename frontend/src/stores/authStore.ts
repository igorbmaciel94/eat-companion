import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthResponse, UserProfile } from '../types';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserProfile | null;
  isAuthenticated: boolean;
  onboardingCompleted: boolean;
  login: (response: AuthResponse) => void;
  logout: () => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  updateUser: (updates: Partial<UserProfile>) => void;
  setOnboardingCompleted: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      onboardingCompleted: false,
      login: (response: AuthResponse) =>
        set({
          accessToken: response.accessToken,
          refreshToken: response.refreshToken,
          user: {
            id: response.user.id,
            email: response.user.email,
            displayName: response.user.displayName,
            calorieTarget: response.user.calorieTarget,
            goalType: response.user.goalType,
            createdAt: response.user.createdAt,
          },
          isAuthenticated: true,
        }),
      logout: () =>
        set({
          accessToken: null,
          refreshToken: null,
          user: null,
          isAuthenticated: false,
          onboardingCompleted: false,
        }),
      setTokens: (accessToken: string, refreshToken: string) =>
        set({ accessToken, refreshToken }),
      updateUser: (updates: Partial<UserProfile>) =>
        set((state) => ({
          user: state.user ? { ...state.user, ...updates } : null,
        })),
      setOnboardingCompleted: () => set({ onboardingCompleted: true }),
    }),
    {
      name: 'auth-storage',
    }
  )
);
