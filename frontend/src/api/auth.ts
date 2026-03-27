import client from './client';
import type { RegisterRequest, LoginRequest, AuthResponse } from '../types';

export const authApi = {
  register: (data: RegisterRequest) =>
    client.post<AuthResponse>('/auth/register', data),
  login: (data: LoginRequest) =>
    client.post<AuthResponse>('/auth/login', data),
  refresh: (refreshToken: string) =>
    client.post<AuthResponse>('/auth/refresh', { refreshToken }),
};
