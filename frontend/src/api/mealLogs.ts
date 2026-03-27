import client from './client';

export const mealLogsApi = {
  log: (data: { date: string; mealType: number; mealOptionId?: string; status: number; notes?: string }) =>
    client.post('/meal-logs', data),
  getByDateRange: (startDate: string, endDate: string) =>
    client.get('/meal-logs', { params: { startDate, endDate } }),
};
