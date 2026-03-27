import client from './client';
import type { UserProfile } from '../types';

export const profileApi = {
  get: () => client.get<UserProfile>('/profile'),
  update: (data: Partial<UserProfile>) =>
    client.put<UserProfile>('/profile', data),
};
