export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    displayName: string;
    calorieTarget?: number;
    goalType?: number;
    createdAt: string;
  };
}

export interface ImportResult {
  mealPlanId: string;
  name: string;
  totalDays: number;
  totalMeals: number;
  totalOptions: number;
}
