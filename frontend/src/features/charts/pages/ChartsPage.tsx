import {
  BarChart,
  Bar,
  XAxis,
  ResponsiveContainer,
  LineChart,
  Line,
  YAxis,
  Tooltip,
} from 'recharts';
import { Icon } from '../../../components/ui/Icon';
import { useAnalytics } from '../hooks/useAnalytics';

export function ChartsPage() {
  const {
    adherenceData,
    weightData,
    averageAdherence,
    currentWeight,
    weeklyWeightChange,
    averageDailyCalories,
  } = useAnalytics();

  const hasData = adherenceData.length > 0 || weightData.length > 0;

  if (!hasData) {
    return (
      <div className="py-2 flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-6">
          <Icon name="analytics" size={40} className="text-primary" />
        </div>
        <h2 className="text-2xl font-headline font-bold text-on-surface mb-2">No data yet</h2>
        <p className="text-on-surface-variant text-base mb-8">
          Start logging meals to see your progress and analytics here.
        </p>
      </div>
    );
  }

  return (
    <div className="py-2">
      {/* Header */}
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-1">
        Analytics
      </p>
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-6">
        Weekly Progress
      </h1>

      {/* Meal Adherence Card */}
      <div className="bg-surface-container-lowest rounded-2xl p-5 editorial-shadow mb-4">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h2 className="font-headline font-semibold text-on-surface text-base mb-0.5">
              Meal Adherence
            </h2>
            <p className="text-on-surface-variant text-xs">
              Percentage of planned meals completed
            </p>
          </div>
          <p className="text-3xl font-headline font-bold text-primary tracking-tight">
            {averageAdherence}%
          </p>
        </div>

        <div className="h-40">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={adherenceData} barCategoryGap="25%">
              <XAxis
                dataKey="day"
                axisLine={false}
                tickLine={false}
                tick={{ fontSize: 11, fill: '#56615f' }}
              />
              <Bar dataKey="value" radius={[6, 6, 0, 0]} fill="#016b61">
                {adherenceData.map((entry, index) => (
                  <rect key={`cell-${index}`} fill={entry.fill} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Weight Trend Card */}
      <div className="bg-surface-container-lowest rounded-2xl p-5 editorial-shadow mb-4">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h2 className="font-headline font-semibold text-on-surface text-base mb-0.5">
              Weight Trend
            </h2>
            <p className="text-on-surface-variant text-xs">This week's measurements</p>
          </div>
          <div className="flex items-center gap-1 bg-primary-container text-on-primary-container px-2.5 py-1 rounded-full">
            <Icon name="monitor_weight" size={14} />
            <span className="text-xs font-medium">{currentWeight} KG</span>
          </div>
        </div>

        <div className="h-40">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={weightData}>
              <XAxis
                dataKey="day"
                axisLine={false}
                tickLine={false}
                tick={{ fontSize: 11, fill: '#56615f' }}
              />
              <YAxis
                hide
                domain={['dataMin - 0.5', 'dataMax + 0.5']}
              />
              <Tooltip
                contentStyle={{
                  background: '#fff',
                  border: 'none',
                  borderRadius: '12px',
                  boxShadow: '0 4px 16px rgba(0,0,0,0.1)',
                  fontSize: '12px',
                }}
                formatter={(value) => [`${value} kg`, 'Weight']}
              />
              <Line
                type="monotone"
                dataKey="weight"
                stroke="#016b61"
                strokeWidth={2.5}
                dot={{ fill: '#016b61', strokeWidth: 0, r: 4 }}
                activeDot={{ r: 6, fill: '#016b61' }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Quick stats */}
      <div className="grid grid-cols-2 gap-3">
        <div className="bg-surface-container-lowest rounded-2xl p-4 editorial-shadow">
          <div className="flex items-center gap-2 mb-2">
            <Icon name="local_fire_department" size={18} className="text-error" />
            <span className="text-on-surface-variant text-xs font-label">Avg Calories</span>
          </div>
          <p className="text-xl font-headline font-bold text-on-surface">{averageDailyCalories.toLocaleString()}</p>
          <p className="text-on-surface-variant text-xs">kcal / day</p>
        </div>
        <div className="bg-surface-container-lowest rounded-2xl p-4 editorial-shadow">
          <div className="flex items-center gap-2 mb-2">
            <Icon name="trending_down" size={18} className="text-primary" />
            <span className="text-on-surface-variant text-xs font-label">Weight Change</span>
          </div>
          <p className="text-xl font-headline font-bold text-on-surface">{weeklyWeightChange}</p>
          <p className="text-on-surface-variant text-xs">kg this week</p>
        </div>
      </div>
    </div>
  );
}
