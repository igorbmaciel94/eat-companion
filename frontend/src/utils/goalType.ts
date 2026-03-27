/**
 * Backend returns goalType as string ("Lose", "Maintain", "Gain") due to JsonStringEnumConverter.
 * Frontend uses number (0, 1, 2) for comparisons and display.
 * This normalizer converts at the API boundary.
 */

const GOAL_TYPE_MAP: Record<string, number> = {
  Lose: 0,
  Maintain: 1,
  Gain: 2,
};

export function normalizeGoalType(value: number | string | undefined | null): number | undefined {
  if (value === undefined || value === null) return undefined;
  if (typeof value === 'number') return value;
  if (typeof value === 'string') return GOAL_TYPE_MAP[value];
  return undefined;
}
