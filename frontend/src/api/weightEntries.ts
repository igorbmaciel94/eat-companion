import client from './client';

export const weightEntriesApi = {
  add: (data: { date: string; weightKg: number }) =>
    client.post('/weight-entries', data),
  getByDateRange: (startDate: string, endDate: string) =>
    client.get('/weight-entries', { params: { startDate, endDate } }),
};
