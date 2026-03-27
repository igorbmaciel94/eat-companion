import client from './client';
import type { UserProfile } from '../types';

export const profileApi = {
  get: () => client.get<UserProfile>('/profile'),
  update: (data: { displayName?: string; calorieTarget?: number; goalType?: number; heightCm?: number; weightKg?: number }) =>
    client.put<UserProfile>('/profile', data),
};
