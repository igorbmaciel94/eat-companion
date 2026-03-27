import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { ProgressBar } from '../../../components/ui/ProgressBar';
import { mealPlansApi } from '../../../api/mealPlans';
import { mealLogsApi } from '../../../api/mealLogs';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';

type MealStatus = 'pending' | 'completed' | 'skipped';

interface Meal {
  type: string;
  name: string;
  calories: number;
  protein?: number;
  status: MealStatus;
  imageUrl?: string;
  mealId?: string;
  optionId?: string;
}


const MEALTYPE_LABELS = ['Breakfast', 'Lunch', 'Snack', 'Dinner'];

export function TodayPage() {
  const navigate = useNavigate();
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const [meals, setMeals] = useState<Meal[]>([]);
  const [loading, setLoading] = useState(false);

  const CALORIE_TARGET = user?.calorieTarget || 2200;
  const PROTEIN_TARGET = 120;

  useEffect(() => {
    if (!isAuthenticated || !activePlanId) return;

    const fetchToday = async () => {
      setLoading(true);
      try {
        const today = new Date().toISOString().split('T')[0];
        const { data } = await mealPlansApi.getDay(activePlanId, today);

        if (data && data.meals && data.meals.length > 0) {
          const apiMeals: Meal[] = data.meals.map((m: { id: string; type?: string; mealType?: number; options?: { id: string; isSelected?: boolean; selected?: boolean; description?: string; name?: string; calories?: number; proteinGrams?: number; protein?: number }[] }) => {
            const selected = m.options?.find((o) => o.isSelected || o.selected) || m.options?.[0];
            return {
              type: MEALTYPE_LABELS[typeof m.mealType === 'number' ? m.mealType : 0] || m.type || 'Meal',
              name: selected?.name || selected?.description?.split('\n')[0] || 'Meal',
              calories: selected?.calories || 0,
              protein: selected?.proteinGrams || selected?.protein || 0,
              status: 'pending' as MealStatus,
              mealId: m.id,
              optionId: selected?.id,
            };
          });
          setMeals(apiMeals);
        }
      } catch {
        // leave meals empty
      } finally {
        setLoading(false);
      }
    };

    fetchToday();
  }, [activePlanId, isAuthenticated, CALORIE_TARGET]);

  const completedCalories = meals
    .filter((m) => m.status === 'completed')
    .reduce((sum, m) => sum + m.calories, 0);

  const completedProtein = meals
    .filter((m) => m.status === 'completed')
    .reduce((sum, m) => sum + (m.protein ?? 0), 0);

  const remaining = CALORIE_TARGET - completedCalories;

  const toggleMeal = async (idx: number) => {
    const meal = meals[idx];
    const newStatus: MealStatus = meal.status === 'completed' ? 'pending' : 'completed';

    setMeals((prev) =>
      prev.map((m, i) =>
        i === idx ? { ...m, status: newStatus } : m,
      ),
    );

    // Log to backend if authenticated
    if (isAuthenticated) {
      try {
        const mealTypeMap: Record<string, number> = { Breakfast: 0, Lunch: 1, Snack: 2, Dinner: 3 };
        await mealLogsApi.log({
          date: new Date().toISOString().split('T')[0],
          mealType: mealTypeMap[meal.type] ?? 0,
          mealOptionId: meal.optionId,
          status: newStatus === 'completed' ? 1 : 0,
        });
      } catch { /* ignore logging errors */ }
    }
  };

  const skipMeal = async (idx: number) => {
    const meal = meals[idx];
    const newStatus: MealStatus = meal.status === 'skipped' ? 'pending' : 'skipped';

    setMeals((prev) =>
      prev.map((m, i) =>
        i === idx ? { ...m, status: newStatus } : m,
      ),
    );

    if (isAuthenticated) {
      try {
        const mealTypeMap: Record<string, number> = { Breakfast: 0, Lunch: 1, Snack: 2, Dinner: 3 };
        await mealLogsApi.log({
          date: new Date().toISOString().split('T')[0],
          mealType: mealTypeMap[meal.type] ?? 0,
          mealOptionId: meal.optionId,
          status: newStatus === 'skipped' ? 2 : 0,
        });
      } catch { /* ignore logging errors */ }
    }
  };

  // Find the first pending meal as the "active" meal
  const activeMealIdx = meals.findIndex((m) => m.status === 'pending');

  // Format today's date
  const today = new Date();
  const dayName = today.toLocaleDateString('en-US', { weekday: 'long' });
  const monthName = today.toLocaleDateString('en-US', { month: 'long' });
  const dayNum = today.getDate();
  const dateLabel = `${dayName}, ${monthName} ${dayNum}`;

  if (loading) {
    return (
      <div className="py-2 flex items-center justify-center min-h-[50vh]">
        <p className="text-on-surface-variant text-sm">Loading today's meals...</p>
      </div>
    );
  }

  if (!activePlanId) {
    return (
      <div className="py-2 flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-6">
          <Icon name="restaurant_menu" size={40} className="text-primary" />
        </div>
        <h2 className="text-2xl font-headline font-bold text-on-surface mb-2">No meal plan yet</h2>
        <p className="text-on-surface-variant text-base mb-8">
          Import your nutritionist's PDF to get started with personalized meal tracking.
        </p>
        <button
          onClick={() => navigate('/plan/import')}
          className="flex items-center gap-2 bg-primary text-on-primary rounded-full px-6 py-3 font-medium text-sm"
        >
          <Icon name="upload_file" size={18} />
          Import meal plan PDF
        </button>
      </div>
    );
  }

  return (
    <div className="py-2">
      {/* Date label */}
      <p className="text-on-surface-variant font-label text-sm uppercase tracking-widest mb-1">
        {dateLabel}
      </p>

      {/* Heading */}
      <h1 className="text-3xl font-headline font-medium tracking-tight text-on-surface mb-5">
        Daily Balance
      </h1>

      {/* Calorie summary card */}
      <div className="bg-surface-container-lowest rounded-2xl p-5 editorial-shadow mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <p className="text-on-surface-variant text-xs font-label uppercase tracking-widest mb-1">
              Remaining Calories
            </p>
            <p className="text-4xl font-headline font-bold text-primary tracking-tight">
              {remaining.toLocaleString()}
            </p>
          </div>
          <div className="text-right">
            <p className="text-on-surface-variant text-xs font-label uppercase tracking-widest mb-1">
              Target
            </p>
            <p className="text-lg font-headline font-semibold text-on-surface">
              {CALORIE_TARGET.toLocaleString()}
            </p>
          </div>
        </div>

        {/* Calories progress */}
        <div className="mb-3">
          <div className="flex justify-between text-xs font-label text-on-surface-variant mb-1">
            <span>Calories</span>
            <span>
              {completedCalories} / {CALORIE_TARGET}
            </span>
          </div>
          <ProgressBar value={completedCalories} max={CALORIE_TARGET} height={6} />
        </div>

        {/* Protein progress */}
        <div>
          <div className="flex justify-between text-xs font-label text-on-surface-variant mb-1">
            <span>Protein</span>
            <span>
              {completedProtein}g / {PROTEIN_TARGET}g
            </span>
          </div>
          <div className="w-full rounded-full bg-surface-container-high overflow-hidden" style={{ height: 6 }}>
            <div
              className="h-full rounded-full bg-tertiary transition-all duration-300 ease-out"
              style={{ width: `${Math.min((completedProtein / PROTEIN_TARGET) * 100, 100)}%` }}
            />
          </div>
        </div>
      </div>

      {/* Planned Meals section */}
      <div className="mb-6">
        <h2 className="text-[10px] font-label font-medium uppercase tracking-widest text-on-surface-variant mb-3">
          Planned Meals
        </h2>

        {meals.length === 0 && (
          <div className="flex flex-col items-center justify-center py-8 text-center">
            <Icon name="calendar_today" size={32} className="text-on-surface-variant/40 mb-3" />
            <p className="text-on-surface-variant text-sm">No meals planned for today.</p>
          </div>
        )}

        <div className="space-y-3">
          {meals.map((meal, idx) => {
            const isActive = idx === activeMealIdx;
            const isCompleted = meal.status === 'completed';
            const isSkipped = meal.status === 'skipped';

            if (isActive) {
              // Expanded active meal card
              return (
                <div
                  key={meal.type + idx}
                  className="bg-surface-container-lowest rounded-2xl overflow-hidden editorial-shadow"
                >
                  {/* Image */}
                  {meal.imageUrl ? (
                    <img
                      src={meal.imageUrl}
                      alt={meal.name}
                      className="w-full h-40 object-cover"
                    />
                  ) : (
                    <div className="w-full h-40 bg-gradient-to-br from-primary-container to-secondary-container flex items-center justify-center">
                      <Icon name="restaurant" size={48} className="text-primary/40" />
                    </div>
                  )}

                  <div className="p-4">
                    <p className="text-[10px] font-label font-medium uppercase tracking-widest text-on-surface-variant mb-0.5">
                      {meal.type}
                    </p>
                    <p className="font-headline font-semibold text-on-surface text-lg">
                      {meal.name}
                    </p>
                    <p className="text-on-surface-variant text-sm mt-1">
                      {meal.calories} kcal
                      {meal.protein ? ` · ${meal.protein}g protein` : ''}
                    </p>

                    <div className="flex items-center gap-2 mt-4">
                      <button
                        onClick={() => toggleMeal(idx)}
                        className="flex-1 flex items-center justify-center gap-2 bg-primary text-on-primary rounded-full py-3 font-medium text-sm transition-colors"
                      >
                        <Icon name="check" size={18} />
                        Complete
                      </button>
                      <button
                        onClick={() => skipMeal(idx)}
                        className="flex items-center justify-center gap-1 border border-outline-variant text-on-surface-variant rounded-full py-3 px-5 text-sm font-medium transition-colors hover:bg-surface-container"
                      >
                        Skip
                      </button>
                    </div>
                  </div>
                </div>
              );
            }

            // Compact card
            return (
              <button
                key={meal.type + idx}
                onClick={() => {
                  if (isCompleted || isSkipped) {
                    // Toggle back to pending
                    setMeals((prev) =>
                      prev.map((m, i) => (i === idx ? { ...m, status: 'pending' } : m)),
                    );
                  }
                }}
                className={`w-full flex items-center gap-3 bg-surface-container-lowest rounded-2xl p-3 editorial-shadow text-left transition-opacity ${
                  isSkipped ? 'opacity-50' : ''
                }`}
              >
                {/* Thumbnail */}
                {meal.imageUrl ? (
                  <img
                    src={meal.imageUrl}
                    alt={meal.name}
                    className="w-14 h-14 rounded-xl object-cover flex-shrink-0"
                  />
                ) : (
                  <div className="w-14 h-14 rounded-xl bg-primary-container/40 flex items-center justify-center flex-shrink-0">
                    <Icon name="restaurant" size={24} className="text-primary/50" />
                  </div>
                )}

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <p className="text-[10px] font-label font-medium uppercase tracking-widest text-on-surface-variant">
                    {meal.type}
                  </p>
                  <p
                    className={`font-headline font-semibold text-on-surface text-sm ${
                      isSkipped ? 'line-through' : ''
                    }`}
                  >
                    {meal.name}
                  </p>
                  <p className="text-on-surface-variant text-xs">{meal.calories} kcal</p>
                </div>

                {/* Status icon */}
                <div className="flex-shrink-0">
                  {isCompleted ? (
                    <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center">
                      <Icon name="check" size={18} className="text-on-primary" />
                    </div>
                  ) : isSkipped ? (
                    <div className="w-8 h-8 rounded-full bg-surface-container-high flex items-center justify-center">
                      <Icon name="remove" size={18} className="text-on-surface-variant" />
                    </div>
                  ) : (
                    <div className="w-8 h-8 rounded-full border-2 border-outline-variant" />
                  )}
                </div>
              </button>
            );
          })}
        </div>
      </div>

      {/* FAB */}
      <button className="fixed bottom-24 right-6 w-14 h-14 rounded-full bg-primary text-on-primary flex items-center justify-center editorial-shadow z-40 transition-transform active:scale-95">
        <Icon name="add" size={28} />
      </button>
    </div>
  );
}
