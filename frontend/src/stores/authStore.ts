import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { AuthResponse, UserProfile } from '../types';
import { normalizeGoalType } from '../utils/goalType';

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
            goalType: normalizeGoalType(response.user.goalType),
            heightCm: response.user.heightCm,
            weightKg: response.user.weightKg,
            onboardingCompleted: response.user.onboardingCompleted,
            createdAt: response.user.createdAt,
          },
          isAuthenticated: true,
          onboardingCompleted: !!response.user.onboardingCompleted,
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
        set((state) => {
          const normalized = updates.goalType !== undefined
            ? { ...updates, goalType: normalizeGoalType(updates.goalType) }
            : updates;
          const newUser = state.user ? { ...state.user, ...normalized } : null;
          return {
            user: newUser,
            onboardingCompleted: newUser?.onboardingCompleted || state.onboardingCompleted,
          };
        }),
      setOnboardingCompleted: () => set({ onboardingCompleted: true }),
    }),
    {
      name: 'auth-storage',
      onRehydrateStorage: () => (state) => {
        // Normalize goalType loaded from localStorage (may be string from backend)
        if (state?.user && state.user.goalType !== undefined) {
          state.user.goalType = normalizeGoalType(state.user.goalType);
        }
      },
    }
  )
);
