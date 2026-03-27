export interface Ingredient {
  id?: string;
  name: string;
  namePt?: string;
  amount?: number;
  quantity?: number;
  unit: string;
  category?: string;
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
  isSelected?: boolean;
  proteinGrams?: number;
}

export interface Meal {
  id: string;
  type: string;
  mealType?: number | string;
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

export interface MealLog {
  id: string;
  date: string;
  mealType: number;
  mealOptionId?: string;
  status: number;
  notes?: string;
}
