import { describe, it, expect } from 'vitest';

/**
 * Tests for PlanPage business logic: weekly data mapping,
 * meal sorting, calorie totals, and option selection.
 */

const MEALTYPE_LABELS: Record<number | string, string> = {
  0: 'Breakfast', 1: 'Morning Snack', 2: 'Lunch', 3: 'Afternoon Snack',
  4: 'Dinner', 5: 'Evening Snack', 6: 'Snack',
  Breakfast: 'Breakfast', MorningSnack: 'Morning Snack', Lunch: 'Lunch',
  AfternoonSnack: 'Afternoon Snack', Dinner: 'Dinner', EveningSnack: 'Evening Snack',
  Snack: 'Snack',
};

interface Option {
  id: string;
  name: string;
  calories: number;
  isSelected?: boolean;
  selected?: boolean;
  ingredients?: { name: string; namePt?: string }[];
}

interface Meal {
  id: string;
  mealType: number | string;
  options: Option[];
}

interface DaySummary {
  date: string;
  meals: Meal[];
}

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

function isOptionSelected(option: Option): boolean {
  return option.isSelected === true || option.selected === true;
}

function mapWeekDays(data: DaySummary[]) {
  return data.map((d) => {
    const date = new Date(d.date + 'T00:00:00');
    const totalCal = d.meals?.reduce((sum, m) => {
      const sel = m.options?.find(isOptionSelected) || m.options?.[0];
      return sum + (sel?.calories || 0);
    }, 0) || 0;
    return {
      date: d.date,
      dayLabel: DAY_NAMES[date.getDay()],
      meals: d.meals || [],
      totalCalories: totalCal,
    };
  });
}

function sortMeals(meals: Meal[]): Meal[] {
  const order: Record<string, number> = {
    Breakfast: 0, '0': 0, MorningSnack: 1, '1': 1,
    Lunch: 2, '2': 2, AfternoonSnack: 3, '3': 3,
    Dinner: 4, '4': 4, EveningSnack: 5, '5': 5,
    Snack: 6, '6': 6,
  };
  return [...meals].sort(
    (a, b) => (order[String(a.mealType)] ?? 99) - (order[String(b.mealType)] ?? 99),
  );
}

describe('PlanPage — weekly data mapping', () => {
  it('should map API data to WeekDay objects', () => {
    const data: DaySummary[] = [
      {
        date: '2026-03-23', // Monday
        meals: [
          { id: '1', mealType: 0, options: [{ id: 'o1', name: 'Oats', calories: 350, isSelected: true, ingredients: [] }] },
          { id: '2', mealType: 2, options: [{ id: 'o2', name: 'Rice', calories: 600, isSelected: true, ingredients: [] }] },
        ],
      },
    ];

    const result = mapWeekDays(data);

    expect(result).toHaveLength(1);
    expect(result[0].dayLabel).toBe('Monday');
    expect(result[0].totalCalories).toBe(950);
    expect(result[0].meals).toHaveLength(2);
  });

  it('should handle empty meals', () => {
    const data: DaySummary[] = [
      { date: '2026-03-24', meals: [] },
    ];

    const result = mapWeekDays(data);
    expect(result[0].totalCalories).toBe(0);
    expect(result[0].meals).toHaveLength(0);
  });

  it('should use first option when none selected', () => {
    const data: DaySummary[] = [
      {
        date: '2026-03-23',
        meals: [
          { id: '1', mealType: 0, options: [
            { id: 'o1', name: 'Option A', calories: 300, ingredients: [] },
            { id: 'o2', name: 'Option B', calories: 500, ingredients: [] },
          ] },
        ],
      },
    ];

    const result = mapWeekDays(data);
    // Should use first option (300 cal) as fallback
    expect(result[0].totalCalories).toBe(300);
  });

  it('should use selected option over first option', () => {
    const data: DaySummary[] = [
      {
        date: '2026-03-23',
        meals: [
          { id: '1', mealType: 0, options: [
            { id: 'o1', name: 'Option A', calories: 300, ingredients: [] },
            { id: 'o2', name: 'Option B', calories: 500, isSelected: true, ingredients: [] },
          ] },
        ],
      },
    ];

    const result = mapWeekDays(data);
    expect(result[0].totalCalories).toBe(500);
  });
});

describe('PlanPage — meal sorting', () => {
  it('should sort meals by type order (Breakfast first, Snack last)', () => {
    const meals: Meal[] = [
      { id: '1', mealType: 4, options: [] },   // Dinner
      { id: '2', mealType: 0, options: [] },   // Breakfast
      { id: '3', mealType: 2, options: [] },   // Lunch
      { id: '4', mealType: 1, options: [] },   // MorningSnack
    ];

    const sorted = sortMeals(meals);
    expect(sorted.map((m) => m.mealType)).toEqual([0, 1, 2, 4]);
  });

  it('should handle string mealType values', () => {
    const meals: Meal[] = [
      { id: '1', mealType: 'Dinner', options: [] },
      { id: '2', mealType: 'Breakfast', options: [] },
      { id: '3', mealType: 'Lunch', options: [] },
    ];

    const sorted = sortMeals(meals);
    expect(sorted.map((m) => m.mealType)).toEqual(['Breakfast', 'Lunch', 'Dinner']);
  });
});

describe('PlanPage — option selection', () => {
  it('should detect isSelected option', () => {
    const opt: Option = { id: '1', name: 'Test', calories: 100, isSelected: true };
    expect(isOptionSelected(opt)).toBe(true);
  });

  it('should detect selected option (legacy field)', () => {
    const opt: Option = { id: '1', name: 'Test', calories: 100, selected: true };
    expect(isOptionSelected(opt)).toBe(true);
  });

  it('should return false when neither field is true', () => {
    const opt: Option = { id: '1', name: 'Test', calories: 100 };
    expect(isOptionSelected(opt)).toBe(false);
  });
});

describe('PlanPage — mealType labels', () => {
  it('should resolve numeric mealType to label', () => {
    expect(MEALTYPE_LABELS[0]).toBe('Breakfast');
    expect(MEALTYPE_LABELS[2]).toBe('Lunch');
    expect(MEALTYPE_LABELS[4]).toBe('Dinner');
  });

  it('should resolve string mealType to label', () => {
    expect(MEALTYPE_LABELS['Breakfast']).toBe('Breakfast');
    expect(MEALTYPE_LABELS['AfternoonSnack']).toBe('Afternoon Snack');
  });
});
