export interface Ingredient {
  name: string;
  quantity: number;
  unit: string;
  calories?: number;
  protein?: number;
  carbs?: number;
  fat?: number;
}

export interface MealOption {
  id: string;
  name: string;
  description?: string;
  ingredients: Ingredient[];
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
  prepTimeMinutes?: number;
  selected: boolean;
}

export interface Meal {
  id: string;
  type: 'breakfast' | 'lunch' | 'dinner' | 'snack';
  time?: string;
  options: MealOption[];
  selectedOptionId?: string;
}

export interface MealPlanDay {
  date: string;
  meals: Meal[];
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
}

export interface MealPlan {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  days: MealPlanDay[];
  createdAt: string;
  updatedAt: string;
}

export interface DailySummary {
  date: string;
  meals: Meal[];
  totalCalories: number;
  totalProtein: number;
  totalCarbs: number;
  totalFat: number;
  calorieGoal: number;
  proteinGoal: number;
  carbsGoal: number;
  fatGoal: number;
}

export interface ImportResult {
  mealPlanId: string;
  name: string;
  daysImported: number;
  mealsImported: number;
}

export interface MealLog {
  id: string;
  mealPlanId: string;
  mealId: string;
  optionId: string;
  date: string;
  logged: boolean;
  loggedAt?: string;
  notes?: string;
}
