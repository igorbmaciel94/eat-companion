import client from './client';
import type { MealPlan, DailySummary } from '../types';

export interface ImportJobResponse {
  jobId: string;
}

export interface ImportJobStatus {
  jobId: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  mealPlanId?: string;
  errorMessage?: string;
}

export const mealPlansApi = {
  import: (file: File) => {
    const form = new FormData();
    form.append('file', file);
    return client.post<ImportJobResponse>('/meal-plans/import', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  },
  getImportStatus: (jobId: string) =>
    client.get<ImportJobStatus>(`/meal-plans/import/${jobId}/status`),
  list: () => client.get<MealPlan[]>('/meal-plans'),
  getById: (id: string) => client.get<MealPlan>(`/meal-plans/${id}`),
  getDay: (id: string, date: string) =>
    client.get<DailySummary>(`/meal-plans/${id}/days/${date}`),
  getWeek: (id: string) =>
    client.get<DailySummary[]>(`/meal-plans/${id}/week`),
  selectOption: (planId: string, mealId: string, optionId: string) =>
    client.put(`/meal-plans/${planId}/meals/${mealId}/options/${optionId}/select`),

  updateOption: (planId: string, optionId: string, data: { name?: string; description?: string; calories?: number; proteinGrams?: number; carbsGrams?: number; fatGrams?: number }) =>
    client.put(`/meal-plans/${planId}/options/${optionId}`, data),

  addIngredient: (planId: string, optionId: string, data: { name: string; namePt?: string; amount: number; unit: string; category: string }) =>
    client.post(`/meal-plans/${planId}/options/${optionId}/ingredients`, data),

  updateIngredient: (planId: string, ingredientId: string, data: { name: string; namePt?: string; amount: number; unit: string; category: string }) =>
    client.put(`/meal-plans/${planId}/ingredients/${ingredientId}`, data),

  deleteIngredient: (planId: string, ingredientId: string) =>
    client.delete(`/meal-plans/${planId}/ingredients/${ingredientId}`),
};
