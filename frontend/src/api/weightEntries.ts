import client from './client';

export const weightEntriesApi = {
  add: (data: { date: string; weightKg: number }) =>
    client.post('/weight-entries', data),
  update: (id: string, data: { weightKg: number; notes?: string }) =>
    client.put(`/weight-entries/${id}`, data),
  getByDateRange: (startDate: string, endDate: string) =>
    client.get('/weight-entries', { params: { startDate, endDate } }),
};
