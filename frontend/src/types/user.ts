export type GoalType = 'lose_weight' | 'gain_muscle' | 'maintain' | 'eat_healthier' | 'save_time';

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  calorieTarget?: number;
  goalType?: number;
  createdAt: string;
}

export interface WeightEntry {
  id: string;
  date: string;
  weightKg: number;
}

export interface WeeklyAnalytics {
  weekStartDate: string;
  weekEndDate: string;
  avgCalories: number;
  avgProtein: number;
  avgCarbs: number;
  avgFat: number;
  adherenceRate: number;
  dailyBreakdown: {
    date: string;
    calories: number;
    protein: number;
    carbs: number;
    fat: number;
    mealsLogged: number;
    mealsPlanned: number;
  }[];
}
