import { useState, useEffect, useCallback } from 'react';
import { analyticsApi } from '../../../api/analytics';
import { useAuthStore } from '../../../stores/authStore';

interface AdherenceDataPoint {
  day: string;
  value: number;
  fill: string;
}

interface WeightDataPoint {
  day: string;
  weight: number;
}

interface AnalyticsData {
  adherenceData: AdherenceDataPoint[];
  weightData: WeightDataPoint[];
  averageAdherence: number;
  currentWeight: number;
  weeklyWeightChange: number;
  averageDailyCalories: number;
}

const FILL_COLORS = ['#9ff2e4', '#016b61'];

interface AnalyticsResult extends AnalyticsData {
  refetch: () => void;
}

export function useAnalytics(): AnalyticsResult {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const [adherenceData, setAdherenceData] = useState<AdherenceDataPoint[]>([]);
  const [weightData, setWeightData] = useState<WeightDataPoint[]>([]);
  const [averageDailyCalories, setAverageDailyCalories] = useState(0);
  const [fetchTrigger, setFetchTrigger] = useState(0);

  const refetch = useCallback(() => setFetchTrigger((n) => n + 1), []);

  useEffect(() => {
    if (!isAuthenticated) return;

    const fetchAnalytics = async () => {
      try {
        const [adherenceRes, weightRes] = await Promise.all([
          analyticsApi.getAdherence(4),
          analyticsApi.getWeightTrend(3),
        ]);

        // Map adherence data from List<WeeklyAnalyticsDto>
        // Backend returns array of weekly summaries with dailySummaries inside
        const weeks = adherenceRes.data;
        if (Array.isArray(weeks) && weeks.length > 0) {
          // Use the most recent week's data for the bar chart
          const recentWeek = weeks[0];
          const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

          // If dailySummaries are populated, use them
          if (recentWeek.dailySummaries && recentWeek.dailySummaries.length > 0) {
            const mapped: AdherenceDataPoint[] = recentWeek.dailySummaries.map(
              (d: { date: string; caloriesConsumed: number; mealsCompleted?: number }, i: number) => {
                const date = new Date(d.date + 'T00:00:00');
                return {
                  day: dayNames[date.getDay()],
                  value: (d.mealsCompleted ?? 0) > 0 || d.caloriesConsumed > 0 ? 100 : 0,
                  fill: FILL_COLORS[i % 2],
                };
              },
            );
            if (mapped.length > 0) setAdherenceData(mapped);

            const totalCal = recentWeek.dailySummaries.reduce(
              (sum: number, d: { caloriesConsumed: number }) => sum + d.caloriesConsumed, 0,
            );
            setAverageDailyCalories(Math.round(totalCal / recentWeek.dailySummaries.length));
          } else {
            // Fallback: create a single bar from the weekly summary
            const total = recentWeek.mealsCompleted + recentWeek.mealsSkipped + recentWeek.mealsSubstituted;
            const adherence = total > 0 ? Math.round((recentWeek.mealsCompleted / total) * 100) : 0;
            if (total > 0) {
              setAdherenceData([{ day: 'This Week', value: adherence, fill: FILL_COLORS[0] }]);
            }
            if (recentWeek.averageCalories > 0) {
              setAverageDailyCalories(recentWeek.averageCalories);
            }
          }
        }

        // Map weight data
        const weightEntries = weightRes.data && Array.isArray(weightRes.data) ? weightRes.data : [];
        const dayNames2 = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

        // Prepend initial weight from user profile if available and not already in entries
        const allWeightPoints: { date: string; weightKg: number }[] = [];
        if (user?.weightKg && user?.createdAt) {
          const startDate = user.createdAt.split('T')[0];
          const alreadyHasStart = weightEntries.some((w: { date: string }) => w.date === startDate);
          if (!alreadyHasStart) {
            allWeightPoints.push({ date: startDate, weightKg: Number(user.weightKg) });
          }
        }
        allWeightPoints.push(...weightEntries);

        if (allWeightPoints.length > 0) {
          const mapped: WeightDataPoint[] = allWeightPoints.slice(-7).map(
            (w) => {
              const date = new Date(w.date + 'T00:00:00');
              return {
                day: dayNames2[date.getDay()],
                weight: w.weightKg,
              };
            },
          );
          if (mapped.length > 0) setWeightData(mapped);
        }
      } catch {
        // Keep mock data on error
      }
    };

    fetchAnalytics();
  }, [isAuthenticated, fetchTrigger]);

  const averageAdherence = adherenceData.length > 0
    ? Math.round(adherenceData.reduce((sum, d) => sum + d.value, 0) / adherenceData.length)
    : 0;

  const currentWeight = weightData.length > 0 ? weightData[weightData.length - 1].weight : 0;
  const weeklyWeightChange = weightData.length > 1
    ? +(currentWeight - weightData[0].weight).toFixed(1)
    : 0;

  return {
    adherenceData,
    weightData,
    averageAdherence,
    currentWeight,
    weeklyWeightChange,
    averageDailyCalories,
    refetch,
  };
}
