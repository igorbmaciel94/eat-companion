import { useState, useEffect } from 'react';
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

const mockAdherenceData: AdherenceDataPoint[] = [
  { day: 'Mon', value: 100, fill: '#9ff2e4' },
  { day: 'Tue', value: 80, fill: '#016b61' },
  { day: 'Wed', value: 90, fill: '#9ff2e4' },
  { day: 'Thu', value: 70, fill: '#016b61' },
  { day: 'Fri', value: 95, fill: '#9ff2e4' },
  { day: 'Sat', value: 60, fill: '#016b61' },
  { day: 'Sun', value: 85, fill: '#9ff2e4' },
];

const mockWeightData: WeightDataPoint[] = [
  { day: 'Mon', weight: 73.2 },
  { day: 'Tue', weight: 73.0 },
  { day: 'Wed', weight: 72.8 },
  { day: 'Thu', weight: 72.9 },
  { day: 'Fri', weight: 72.6 },
  { day: 'Sat', weight: 72.5 },
  { day: 'Sun', weight: 72.4 },
];

const FILL_COLORS = ['#9ff2e4', '#016b61'];

export function useAnalytics(): AnalyticsData {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [adherenceData, setAdherenceData] = useState<AdherenceDataPoint[]>(mockAdherenceData);
  const [weightData, setWeightData] = useState<WeightDataPoint[]>(mockWeightData);
  const [averageDailyCalories, setAverageDailyCalories] = useState(1820);

  useEffect(() => {
    if (!isAuthenticated) return;

    const fetchAnalytics = async () => {
      try {
        const [adherenceRes, weightRes] = await Promise.all([
          analyticsApi.getAdherence(4),
          analyticsApi.getWeightTrend(3),
        ]);

        // Map adherence data
        if (adherenceRes.data?.dailyBreakdown && adherenceRes.data.dailyBreakdown.length > 0) {
          const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
          const mapped: AdherenceDataPoint[] = adherenceRes.data.dailyBreakdown.slice(-7).map(
            (d: { date: string; mealsLogged: number; mealsPlanned: number; calories: number }, i: number) => {
              const date = new Date(d.date + 'T00:00:00');
              const adherence = d.mealsPlanned > 0 ? Math.round((d.mealsLogged / d.mealsPlanned) * 100) : 0;
              return {
                day: dayNames[date.getDay()],
                value: adherence,
                fill: FILL_COLORS[i % 2],
              };
            },
          );
          if (mapped.length > 0) setAdherenceData(mapped);

          // Avg calories
          const totalCal = adherenceRes.data.dailyBreakdown.reduce(
            (sum: number, d: { calories: number }) => sum + d.calories, 0,
          );
          if (adherenceRes.data.dailyBreakdown.length > 0) {
            setAverageDailyCalories(Math.round(totalCal / adherenceRes.data.dailyBreakdown.length));
          }
        }

        // Map weight data
        if (weightRes.data && Array.isArray(weightRes.data) && weightRes.data.length > 0) {
          const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
          const mapped: WeightDataPoint[] = weightRes.data.slice(-7).map(
            (w: { date: string; weightKg: number }) => {
              const date = new Date(w.date + 'T00:00:00');
              return {
                day: dayNames[date.getDay()],
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
  }, [isAuthenticated]);

  const averageAdherence = Math.round(
    adherenceData.reduce((sum, d) => sum + d.value, 0) / adherenceData.length,
  );

  const currentWeight = weightData[weightData.length - 1].weight;
  const weeklyWeightChange = +(currentWeight - weightData[0].weight).toFixed(1);

  return {
    adherenceData,
    weightData,
    averageAdherence,
    currentWeight,
    weeklyWeightChange,
    averageDailyCalories,
  };
}
