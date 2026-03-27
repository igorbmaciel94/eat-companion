import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface UiState {
  activeMealPlanId: string | null;
  setActiveMealPlanId: (id: string | null) => void;
}

export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      activeMealPlanId: null,
      setActiveMealPlanId: (id: string | null) => set({ activeMealPlanId: id }),
    }),
    {
      name: 'ui-storage',
    }
  )
);
