import client from './client';
import type { MealLog } from '../types';

export const mealLogsApi = {
  log: (data: { mealPlanId: string; mealId: string; optionId: string; date: string; notes?: string }) =>
    client.post<MealLog>('/meal-logs', data),
  getByDate: (date: string) =>
    client.get<MealLog[]>('/meal-logs', { params: { date } }),
};
