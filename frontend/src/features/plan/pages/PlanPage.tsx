import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { mealPlansApi } from '../../../api/mealPlans';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';

type ViewMode = 'today' | 'weekly';

interface PlanMeal {
  type: string;
  name: string;
  calories: number;
  protein: number;
  imageUrl?: string;
}

const mockMeals: PlanMeal[] = [
  {
    type: 'Breakfast',
    name: 'Avocado Toast with Poached Eggs',
    calories: 380,
    protein: 18,
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuAP6iNvLxkxyoSS9b0kpeQgIozTTTQ_HuEmwzUCJGLGx0W0nOzzCI_us9_8UG-5Yeay6WoxVD3sXec2TXfBnaxjGv5cj8dRqrjbtkr0qa8UVkrJV9FhP4QUrzXgxMS1KBVSC4SGlRmpxN3y29QODKw9QwN6m9UplhksXWs2wvboQFFWOFsz64-Y2_89dusSSyY3SOyYDnqvMVVI8i1asM4HWqu-13dFeWNETo2Dnn-K9NDLfDug0m5gu7f1m-tEEbw9lz1BSIxCWPsO',
  },
  {
    type: 'Lunch',
    name: 'Garden Quinoa Bowl',
    calories: 520,
    protein: 24,
    imageUrl:
      'https://lh3.googleusercontent.com/aida-public/AB6AXuDgDQSGe_ySpC4yS3ntEY5XiQmUVaMsuCCXVIzIrTzXGBYcDrGyM7YorM0UVB0L40mNdcruPxwLLELTuDPq4k18zmApAJMgFuda5EkUShN5Dm7liPgqgjKqAI_r-YmflIk5zL2kPbdD2-iTd-_t25wrzgYd32q_-kSvF-6pkUzgxMEsHXeu1USLbK-h3PoYtWuhLvQbfKYdrVLCRWOutX_xE0CHWHB9VnwxxT4QtYk1BKFkI7sU1RcZd0oxf9Cvildhdcvqbt3mhdvr',
  },
  {
    type: 'Dinner',
    name: 'Grilled Salmon with Vegetables',
    calories: 480,
    protein: 35,
  },
];

const MEALTYPE_LABELS = ['Breakfast', 'Lunch', 'Snack', 'Dinner'];

export function PlanPage() {
  const navigate = useNavigate();
  const [viewMode, setViewMode] = useState<ViewMode>('today');
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const [meals, setMeals] = useState<PlanMeal[]>(mockMeals);
  const [loading, setLoading] = useState(false);

  const CALORIE_TARGET = user?.calorieTarget || 2200;

  useEffect(() => {
    if (!isAuthenticated || !activePlanId) return;

    const fetchPlan = async () => {
      setLoading(true);
      try {
        const today = new Date().toISOString().split('T')[0];
        const { data } = await mealPlansApi.getDay(activePlanId, today);

        if (data && data.meals && data.meals.length > 0) {
          const apiMeals: PlanMeal[] = data.meals.map((m: { type?: string; mealType?: number; options?: { isSelected?: boolean; selected?: boolean; description?: string; name?: string; calories?: number; proteinGrams?: number; protein?: number }[] }) => {
            const selected = m.options?.find((o) => o.isSelected || o.selected) || m.options?.[0];
            return {
              type: MEALTYPE_LABELS[typeof m.mealType === 'number' ? m.mealType : 0] || m.type || 'Meal',
              name: selected?.description || selected?.name || 'Meal',
              calories: selected?.calories || 0,
              protein: selected?.proteinGrams || selected?.protein || 0,
            };
          });
          setMeals(apiMeals);
        }
      } catch {
        // Keep mock data on error
      } finally {
        setLoading(false);
      }
    };

    fetchPlan();
  }, [activePlanId, isAuthenticated, CALORIE_TARGET]);

  const totalCalories = meals.reduce((sum, m) => sum + m.calories, 0);
  const remaining = CALORIE_TARGET - totalCalories;

  // Format today's date
  const today = new Date();
  const dayNum = today.getDate();
  const monthYear = today.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  const dayName = today.toLocaleDateString('en-US', { weekday: 'long' });

  if (loading) {
    return (
      <div className="py-2 flex items-center justify-center min-h-[50vh]">
        <p className="text-on-surface-variant text-sm">Loading plan...</p>
      </div>
    );
  }

  return (
    <div className="py-2">
      {/* Toggle pills */}
      <div className="flex gap-2 mb-6">
        <button
          onClick={() => setViewMode('today')}
          className={`px-5 py-2 rounded-full text-sm font-medium transition-colors ${
            viewMode === 'today'
              ? 'bg-primary text-on-primary'
              : 'bg-surface-container text-on-surface-variant'
          }`}
        >
          Today
        </button>
        <button
          onClick={() => setViewMode('weekly')}
          className={`px-5 py-2 rounded-full text-sm font-medium transition-colors ${
            viewMode === 'weekly'
              ? 'bg-primary text-on-primary'
              : 'bg-surface-container text-on-surface-variant'
          }`}
        >
          Weekly
        </button>
      </div>

      {/* Date display */}
      <div className="mb-6">
        <div className="flex items-baseline gap-3">
          <span className="text-5xl font-headline font-bold text-on-surface tracking-tight">{dayNum}</span>
          <div>
            <p className="text-on-surface-variant font-label text-sm uppercase tracking-widest">{monthYear}</p>
            <p className="text-on-surface font-headline font-medium text-lg">{dayName}</p>
          </div>
        </div>
      </div>

      {/* Day summary */}
      <div className="flex items-center justify-between mb-5">
        <p className="font-headline font-semibold text-on-surface text-base">Today's Plan</p>
        <p className="text-on-surface-variant text-sm font-label">
          {remaining > 0 ? `${remaining} kcal remaining` : 'Target reached'}
        </p>
      </div>

      {/* Meal cards */}
      <div className="space-y-3 mb-6">
        {meals.map((meal, idx) => (
          <div
            key={meal.type + idx}
            className="bg-surface-container-lowest rounded-2xl overflow-hidden editorial-shadow"
          >
            <div className="flex gap-3 p-3">
              {/* Thumbnail */}
              {meal.imageUrl ? (
                <img
                  src={meal.imageUrl}
                  alt={meal.name}
                  className="w-20 h-20 rounded-xl object-cover flex-shrink-0"
                />
              ) : (
                <div className="w-20 h-20 rounded-xl bg-gradient-to-br from-primary-container to-secondary-container flex items-center justify-center flex-shrink-0">
                  <Icon name="restaurant" size={32} className="text-primary/40" />
                </div>
              )}

              {/* Info */}
              <div className="flex-1 min-w-0 py-0.5">
                <p className="text-[10px] font-label font-medium uppercase tracking-widest text-on-surface-variant mb-0.5">
                  {meal.type}
                </p>
                <p className="font-headline font-semibold text-on-surface text-sm mb-1">
                  {meal.name}
                </p>
                <span className="inline-block bg-primary-container text-on-primary-container text-xs font-label font-medium px-2 py-0.5 rounded-full">
                  {meal.calories} kcal
                </span>
              </div>

              {/* Edit button */}
              <button className="self-start p-1.5 rounded-full hover:bg-surface-container transition-colors flex-shrink-0">
                <Icon name="edit" size={18} className="text-on-surface-variant" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Import / Add button */}
      <button
        onClick={() => navigate('/plan/import')}
        className="w-full flex items-center justify-center gap-2 border-2 border-dashed border-outline-variant rounded-2xl py-4 text-on-surface-variant text-sm font-medium transition-colors hover:border-primary hover:text-primary"
      >
        <Icon name="upload_file" size={20} />
        Import PDF or Add Meal
      </button>
    </div>
  );
}
