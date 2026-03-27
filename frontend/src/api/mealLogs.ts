import client from './client';

export const mealLogsApi = {
  log: (data: { date: string; mealType: number | string; mealOptionId?: string; status: number | string; notes?: string }) =>
    client.post('/meal-logs', data),
  getByDateRange: (startDate: string, endDate: string) =>
    client.get('/meal-logs', { params: { startDate, endDate } }),
  delete: (date: string, mealOptionId?: string) =>
    client.delete('/meal-logs', { params: { date, mealOptionId } }),
};
