import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { mealPlansApi } from '../../../api/mealPlans';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';
import type { Meal, MealOption } from '../../../types';

type ViewMode = 'today' | 'weekly';

const MEALTYPE_LABELS = ['Breakfast', 'Lunch', 'Snack', 'Dinner'];

function getOptionName(option: MealOption): string {
  if (option.name) return option.name;
  if (option.description) {
    const firstLine = option.description.split('\n')[0].trim();
    if (firstLine) return firstLine;
  }
  return 'Option';
}

function getIngredientLines(option: MealOption): string[] {
  if (option.description) {
    return option.description
      .split('\n')
      .map((line) => line.trim())
      .filter(Boolean);
  }
  if (option.ingredients?.length) {
    return option.ingredients.map(
      (ing) => `${ing.quantity} ${ing.unit} ${ing.name}`.trim()
    );
  }
  return [];
}

function isOptionSelected(option: MealOption): boolean {
  return option.isSelected === true || option.selected === true;
}

export function PlanPage() {
  const navigate = useNavigate();
  const [viewMode, setViewMode] = useState<ViewMode>('today');
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const [meals, setMeals] = useState<Meal[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectingOption, setSelectingOption] = useState<string | null>(null);

  const calorieTarget = user?.calorieTarget || 2200;

  const fetchDay = useCallback(async () => {
    if (!isAuthenticated || !activePlanId) return;
    setLoading(true);
    try {
      const today = new Date().toISOString().split('T')[0];
      const { data } = await mealPlansApi.getDay(activePlanId, today);
      if (data?.meals?.length) {
        setMeals(data.meals);
      }
    } catch {
      // keep current state on error
    } finally {
      setLoading(false);
    }
  }, [activePlanId, isAuthenticated]);

  useEffect(() => {
    fetchDay();
  }, [fetchDay]);

  const handleSelectOption = async (meal: Meal, option: MealOption) => {
    if (!activePlanId || isOptionSelected(option)) return;
    setSelectingOption(option.id);
    try {
      await mealPlansApi.selectOption(activePlanId, meal.id, option.id);
      setMeals((prev) =>
        prev.map((m) => {
          if (m.id !== meal.id) return m;
          return {
            ...m,
            selectedOptionId: option.id,
            options: m.options.map((o) => ({
              ...o,
              isSelected: o.id === option.id,
              selected: o.id === option.id,
            })),
          };
        })
      );
    } catch {
      // keep current state on error
    } finally {
      setSelectingOption(null);
    }
  };

  const totalCalories = meals.reduce((sum, m) => {
    const selected = m.options.find(isOptionSelected) || m.options[0];
    return sum + (selected?.calories || 0);
  }, 0);
  const remaining = calorieTarget - totalCalories;

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

  if (!activePlanId) {
    return (
      <div className="py-2 flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-6">
          <Icon name="restaurant_menu" size={40} className="text-primary" />
        </div>
        <h2 className="text-2xl font-headline font-bold text-on-surface mb-2">No meal plan yet</h2>
        <p className="text-on-surface-variant text-base mb-8">
          Import your nutritionist's PDF to get started with personalized meal planning.
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

      {viewMode === 'today' ? (
        <>
          {/* Date display */}
          <div className="mb-6">
            <div className="flex items-baseline gap-3">
              <span className="text-5xl font-headline font-bold text-on-surface tracking-tight">
                {dayNum}
              </span>
              <div>
                <p className="text-on-surface-variant font-label text-sm uppercase tracking-widest">
                  {monthYear}
                </p>
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

          {/* Calorie overview */}
          <div className="bg-surface-container-lowest rounded-2xl editorial-shadow p-4 mb-6">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm font-label text-on-surface-variant">Calories</span>
              <span className="text-sm font-label text-on-surface-variant">
                {totalCalories} / {calorieTarget} kcal
              </span>
            </div>
            <div className="w-full h-2 bg-surface-container rounded-full overflow-hidden">
              <div
                className={`h-full rounded-full transition-all duration-300 ${
                  totalCalories > calorieTarget ? 'bg-error' : 'bg-primary'
                }`}
                style={{ width: `${Math.min((totalCalories / calorieTarget) * 100, 100)}%` }}
              />
            </div>
          </div>

          {/* Meal sections */}
          {meals.length === 0 && (
            <div className="flex flex-col items-center justify-center py-8 text-center">
              <Icon name="calendar_today" size={32} className="text-on-surface-variant/40 mb-3" />
              <p className="text-on-surface-variant text-sm">No meals planned for today.</p>
            </div>
          )}

          <div className="space-y-6 mb-6">
            {meals.map((meal) => {
              const mealLabel =
                MEALTYPE_LABELS[typeof meal.mealType === 'number' ? meal.mealType : 0] ||
                meal.type ||
                'Meal';

              return (
                <div key={meal.id}>
                  <p className="text-xs font-label font-medium uppercase tracking-widest text-on-surface-variant mb-2">
                    {mealLabel}
                  </p>
                  <div className="space-y-2">
                    {meal.options.map((option) => {
                      const selected = isOptionSelected(option);
                      const ingredientLines = getIngredientLines(option);
                      const isSelecting = selectingOption === option.id;

                      return (
                        <button
                          key={option.id}
                          onClick={() => handleSelectOption(meal, option)}
                          disabled={isSelecting}
                          className={`w-full text-left rounded-2xl p-3 transition-all ${
                            selected
                              ? 'bg-primary-container border-2 border-primary editorial-shadow'
                              : 'bg-surface-container-lowest border-2 border-transparent editorial-shadow hover:border-outline-variant'
                          } ${isSelecting ? 'opacity-60' : ''}`}
                        >
                          <div className="flex items-start justify-between gap-2">
                            <div className="flex-1 min-w-0">
                              <p
                                className={`font-headline font-semibold text-sm mb-1 ${
                                  selected ? 'text-on-primary-container' : 'text-on-surface'
                                }`}
                              >
                                {getOptionName(option)}
                              </p>

                              {ingredientLines.length > 0 && (
                                <ul className="space-y-0.5 mb-2">
                                  {ingredientLines.map((line, i) => (
                                    <li
                                      key={i}
                                      className={`text-xs flex items-start gap-1.5 ${
                                        selected
                                          ? 'text-on-primary-container/70'
                                          : 'text-on-surface-variant'
                                      }`}
                                    >
                                      <span className="mt-1.5 w-1 h-1 rounded-full bg-current flex-shrink-0" />
                                      {line}
                                    </li>
                                  ))}
                                </ul>
                              )}
                            </div>

                            <div className="flex flex-col items-end gap-1.5 flex-shrink-0">
                              {selected && (
                                <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center">
                                  <Icon name="check" size={14} className="text-on-primary" />
                                </div>
                              )}
                              {option.calories > 0 && (
                                <span
                                  className={`text-xs font-label font-medium px-2 py-0.5 rounded-full ${
                                    selected
                                      ? 'bg-primary text-on-primary'
                                      : 'bg-primary-container text-on-primary-container'
                                  }`}
                                >
                                  {option.calories} kcal
                                </span>
                              )}
                            </div>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </>
      ) : (
        /* Weekly view placeholder */
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="w-16 h-16 rounded-full bg-primary-container flex items-center justify-center mb-4">
            <Icon name="date_range" size={28} className="text-primary" />
          </div>
          <p className="text-on-surface font-headline font-semibold text-base mb-1">
            Same plan for all days
          </p>
          <p className="text-on-surface-variant text-sm">
            Your meal plan repeats daily. Switch to Today to see your options.
          </p>
        </div>
      )}

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
