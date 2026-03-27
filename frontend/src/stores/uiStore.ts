import { create } from 'zustand';

interface UiState {
  activeMealPlanId: string | null;
  setActiveMealPlanId: (id: string | null) => void;
}

export const useUiStore = create<UiState>()((set) => ({
  activeMealPlanId: null,
  setActiveMealPlanId: (id: string | null) => set({ activeMealPlanId: id }),
}));
