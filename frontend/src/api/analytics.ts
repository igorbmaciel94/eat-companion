import client from './client';
import type { WeeklyAnalytics } from '../types';

export const analyticsApi = {
  getWeekly: (startDate: string) =>
    client.get<WeeklyAnalytics>('/analytics/weekly', { params: { startDate } }),
};
