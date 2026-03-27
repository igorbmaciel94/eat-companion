import { describe, it, expect } from 'vitest';

/**
 * Tests for TodayPage business logic extracted from the component.
 * These test the hydration, calorie calculation, and status mapping logic.
 */

const MEALTYPE_STRING: Record<number, string> = {
  0: 'Breakfast', 1: 'MorningSnack', 2: 'Lunch', 3: 'AfternoonSnack',
  4: 'Dinner', 5: 'EveningSnack', 6: 'Snack',
};

const MEALTYPE_NUMBER: Record<string, number> = {
  Breakfast: 0, MorningSnack: 1, Lunch: 2, AfternoonSnack: 3,
  Dinner: 4, EveningSnack: 5, Snack: 6,
};

function getMealTypeString(mt: number | string | undefined): string {
  if (typeof mt === 'string' && MEALTYPE_NUMBER[mt] !== undefined) return mt;
  if (typeof mt === 'number') return MEALTYPE_STRING[mt] ?? 'Breakfast';
  return 'Breakfast';
}

type MealStatus = 'pending' | 'completed' | 'skipped';

interface Log {
  mealOptionId?: string;
  mealType: string | number;
  status: string | number;
}

function hydrateStatus(logs: Log[], selectedId: string | undefined, mealTypeStr: string): MealStatus {
  const normalizedSelectedId = selectedId?.toLowerCase();

  // Primary match: by mealOptionId
  let log = logs.find(
    (l) => l.mealOptionId && normalizedSelectedId && l.mealOptionId.toLowerCase() === normalizedSelectedId,
  );

  // Fallback match: by mealType
  if (!log) {
    log = logs.find((l) => {
      const logMealType = typeof l.mealType === 'string' ? l.mealType : MEALTYPE_STRING[l.mealType] ?? '';
      return logMealType === mealTypeStr;
    });
  }

  if (!log) return 'pending';

  const logStatus = typeof log.status === 'string' ? log.status : '';
  if (log.status === 0 || logStatus === 'Completed') return 'completed';
  if (log.status === 1 || logStatus === 'Skipped') return 'skipped';
  return 'pending';
}

describe('TodayPage — getMealTypeString', () => {
  it('should convert number to string', () => {
    expect(getMealTypeString(0)).toBe('Breakfast');
    expect(getMealTypeString(2)).toBe('Lunch');
    expect(getMealTypeString(4)).toBe('Dinner');
  });

  it('should return string as-is if valid', () => {
    expect(getMealTypeString('Breakfast')).toBe('Breakfast');
    expect(getMealTypeString('Lunch')).toBe('Lunch');
  });

  it('should fallback to Breakfast for undefined', () => {
    expect(getMealTypeString(undefined)).toBe('Breakfast');
  });
});

describe('TodayPage — meal status hydration', () => {
  it('should match log by mealOptionId (case-insensitive)', () => {
    const logs: Log[] = [
      { mealOptionId: 'ABC-123', mealType: 'Breakfast', status: 'Completed' },
    ];

    const status = hydrateStatus(logs, 'abc-123', 'Breakfast');
    expect(status).toBe('completed');
  });

  it('should fallback to mealType match when optionId does not match', () => {
    const logs: Log[] = [
      { mealOptionId: 'different-id', mealType: 'Lunch', status: 'Completed' },
    ];

    const status = hydrateStatus(logs, 'my-option-id', 'Lunch');
    expect(status).toBe('completed');
  });

  it('should handle string status "Completed"', () => {
    const logs: Log[] = [
      { mealOptionId: 'opt-1', mealType: 'Breakfast', status: 'Completed' },
    ];
    expect(hydrateStatus(logs, 'opt-1', 'Breakfast')).toBe('completed');
  });

  it('should handle numeric status 0 (Completed)', () => {
    const logs: Log[] = [
      { mealOptionId: 'opt-1', mealType: 'Breakfast', status: 0 },
    ];
    expect(hydrateStatus(logs, 'opt-1', 'Breakfast')).toBe('completed');
  });

  it('should handle string status "Skipped"', () => {
    const logs: Log[] = [
      { mealOptionId: 'opt-1', mealType: 'Dinner', status: 'Skipped' },
    ];
    expect(hydrateStatus(logs, 'opt-1', 'Dinner')).toBe('skipped');
  });

  it('should handle numeric status 1 (Skipped)', () => {
    const logs: Log[] = [
      { mealOptionId: 'opt-1', mealType: 'Dinner', status: 1 },
    ];
    expect(hydrateStatus(logs, 'opt-1', 'Dinner')).toBe('skipped');
  });

  it('should return pending when no matching log', () => {
    const logs: Log[] = [
      { mealOptionId: 'other', mealType: 'Breakfast', status: 'Completed' },
    ];
    expect(hydrateStatus(logs, 'my-opt', 'Dinner')).toBe('pending');
  });

  it('should return pending for empty logs', () => {
    expect(hydrateStatus([], 'opt-1', 'Lunch')).toBe('pending');
  });

  it('should match mealType with numeric log mealType', () => {
    const logs: Log[] = [
      { mealOptionId: 'wrong-id', mealType: 2, status: 'Completed' },
    ];

    // mealType 2 = Lunch in MEALTYPE_STRING
    const status = hydrateStatus(logs, 'my-id', 'Lunch');
    expect(status).toBe('completed');
  });
});

describe('TodayPage — calorie calculations', () => {
  it('should sum only completed meal calories', () => {
    const meals = [
      { calories: 350, status: 'completed' as MealStatus },
      { calories: 600, status: 'completed' as MealStatus },
      { calories: 200, status: 'skipped' as MealStatus },
      { calories: 400, status: 'pending' as MealStatus },
    ];

    const completedCalories = meals
      .filter((m) => m.status === 'completed')
      .reduce((sum, m) => sum + m.calories, 0);

    expect(completedCalories).toBe(950);
  });

  it('should detect allHandled correctly', () => {
    const allDone = [
      { status: 'completed' as MealStatus },
      { status: 'skipped' as MealStatus },
      { status: 'completed' as MealStatus },
    ];
    expect(allDone.every((m) => m.status !== 'pending')).toBe(true);

    const notDone = [
      { status: 'completed' as MealStatus },
      { status: 'pending' as MealStatus },
    ];
    expect(notDone.every((m) => m.status !== 'pending')).toBe(false);
  });

  it('should return false for allHandled with empty meals', () => {
    const meals: { status: MealStatus }[] = [];
    const allHandled = meals.length > 0 && meals.every((m) => m.status !== 'pending');
    expect(allHandled).toBe(false);
  });
});
