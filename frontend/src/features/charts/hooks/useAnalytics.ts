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

const adherenceData: AdherenceDataPoint[] = [
  { day: 'Mon', value: 100, fill: '#9ff2e4' },
  { day: 'Tue', value: 80, fill: '#016b61' },
  { day: 'Wed', value: 90, fill: '#9ff2e4' },
  { day: 'Thu', value: 70, fill: '#016b61' },
  { day: 'Fri', value: 95, fill: '#9ff2e4' },
  { day: 'Sat', value: 60, fill: '#016b61' },
  { day: 'Sun', value: 85, fill: '#9ff2e4' },
];

const weightData: WeightDataPoint[] = [
  { day: 'Mon', weight: 73.2 },
  { day: 'Tue', weight: 73.0 },
  { day: 'Wed', weight: 72.8 },
  { day: 'Thu', weight: 72.9 },
  { day: 'Fri', weight: 72.6 },
  { day: 'Sat', weight: 72.5 },
  { day: 'Sun', weight: 72.4 },
];

export function useAnalytics(): AnalyticsData {
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
    averageDailyCalories: 1820,
  };
}
