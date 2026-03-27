import client from './client';
import type { WeightEntry } from '../types';

export const weightEntriesApi = {
  list: () => client.get<WeightEntry[]>('/weight-entries'),
  create: (data: { date: string; weight: number; unit: 'kg' | 'lbs'; notes?: string }) =>
    client.post<WeightEntry>('/weight-entries', data),
};
