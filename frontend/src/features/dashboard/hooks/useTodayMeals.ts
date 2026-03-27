import { useState } from 'react';

type MealStatus = 'pending' | 'completed' | 'skipped';

interface Meal {
  id: string;
  type: string;
  name: string;
  calories: number;
  protein: number;
  status: MealStatus;
  imageUrl?: string;
}

interface DailySummary {
  date: string;
  calorieTarget: number;
  proteinTarget: number;
  meals: Meal[];
  caloriesConsumed: number;
  proteinConsumed: number;
  remainingCalories: number;
}

const mockMeals: Meal[] = [
  {
    id: '1',
    type: 'Breakfast',
    name: 'Avocado Toast',
    calories: 340,
    protein: 12,
    status: 'pending',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAP6iNvLxkxyoSS9b0kpeQgIozTTTQ_HuEmwzUCJGLGx0W0nOzzCI_us9_8UG-5Yeay6WoxVD3sXec2TXfBnaxjGv5cj8dRqrjbtkr0qa8UVkrJV9FhP4QUrzXgxMS1KBVSC4SGlRmpxN3y29QODKw9QwN6m9UplhksXWs2wvboQFFWOFsz64-Y2_89dusSSyY3SOyYDnqvMVVI8i1asM4HWqu-13dFeWNETo2Dnn-K9NDLfDug0m5gu7f1m-tEEbw9lz1BSIxCWPsO',
  },
  {
    id: '2',
    type: 'Lunch',
    name: 'Garden Quinoa Bowl',
    calories: 520,
    protein: 24,
    status: 'pending',
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuDgDQSGe_ySpC4yS3ntEY5XiQmUVaMsuCCXVIzIrTzXGBYcDrGyM7YorM0UVB0L40mNdcruPxwLLELTuDPq4k18zmApAJMgFuda5EkUShN5Dm7liPgqgjKqAI_r-YmflIk5zL2kPbdD2-iTd-_t25wrzgYd32q_-kSvF-6pkUzgxMEsHXeu1USLbK-h3PoYtWuhLvQbfKYdrVLCRWOutX_xE0CHWHB9VnwxxT4QtYk1BKFkI7sU1RcZd0oxf9Cvildhdcvqbt3mhdvr',
  },
  {
    id: '3',
    type: 'Dinner',
    name: 'Grilled Salmon with Vegetables',
    calories: 480,
    protein: 35,
    status: 'pending',
  },
];

export function useTodayMeals() {
  const [meals, setMeals] = useState<Meal[]>(mockMeals);
  const calorieTarget = 2200;
  const proteinTarget = 120;

  const completedMeals = meals.filter((m) => m.status === 'completed');
  const caloriesConsumed = completedMeals.reduce((sum, m) => sum + m.calories, 0);
  const proteinConsumed = completedMeals.reduce((sum, m) => sum + m.protein, 0);

  const toggleMeal = (id: string) => {
    setMeals((prev) =>
      prev.map((m) =>
        m.id === id
          ? { ...m, status: m.status === 'completed' ? 'pending' : 'completed' }
          : m,
      ),
    );
  };

  const skipMeal = (id: string) => {
    setMeals((prev) =>
      prev.map((m) =>
        m.id === id
          ? { ...m, status: m.status === 'skipped' ? 'pending' : 'skipped' }
          : m,
      ),
    );
  };

  const summary: DailySummary = {
    date: 'Monday, May 12',
    calorieTarget,
    proteinTarget,
    meals,
    caloriesConsumed,
    proteinConsumed,
    remainingCalories: calorieTarget - caloriesConsumed,
  };

  return {
    summary,
    meals,
    toggleMeal,
    skipMeal,
  };
}
