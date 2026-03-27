import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface UiState {
  activeMealPlanId: string | null;
  pendingImportJobId: string | null;
  setActiveMealPlanId: (id: string | null) => void;
  setPendingImportJobId: (id: string | null) => void;
  clearPendingImportJobId: () => void;
}

export const useUiStore = create<UiState>()(
  persist(
    (set) => ({
      activeMealPlanId: null,
      pendingImportJobId: null,
      setActiveMealPlanId: (id: string | null) => set({ activeMealPlanId: id }),
      setPendingImportJobId: (id: string | null) => set({ pendingImportJobId: id }),
      clearPendingImportJobId: () => set({ pendingImportJobId: null }),
    }),
    {
      name: 'ui-storage',
    }
  )
);
