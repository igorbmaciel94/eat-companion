import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { mealPlansApi } from '../../../api/mealPlans';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';
import type { Meal, MealOption, Ingredient } from '../../../types';

type ViewMode = 'today' | 'weekly';

const MEALTYPE_LABELS: Record<number | string, string> = {
  0: 'Breakfast', 1: 'Morning Snack', 2: 'Lunch', 3: 'Afternoon Snack',
  4: 'Dinner', 5: 'Evening Snack', 6: 'Snack',
  Breakfast: 'Breakfast', MorningSnack: 'Morning Snack', Lunch: 'Lunch',
  AfternoonSnack: 'Afternoon Snack', Dinner: 'Dinner', EveningSnack: 'Evening Snack',
  Snack: 'Snack',
};

const UNIT_OPTIONS = ['Grams', 'Ml', 'Tablespoon', 'Teaspoon', 'Slice', 'Units'];
const CATEGORY_OPTIONS = ['Protein', 'Dairy', 'Grains', 'Produce', 'OilsAndCondiments', 'Other'];

function getOptionName(option: MealOption): string {
  if (option.name) return option.name;
  if (option.description) {
    const firstLine = option.description.split('\n')[0].trim();
    if (firstLine) return firstLine;
  }
  return 'Option';
}

function getIngredientLines(option: MealOption): string[] {
  // Prefer structured ingredients from Claude API
  if (option.ingredients?.length) {
    return option.ingredients.map((ing) => {
      const amount = ing.amount || ing.quantity;
      const unit = ing.unit;
      const name = ing.namePt || ing.name;
      if (!amount) return name;
      if (unit === 'Grams') return `${amount}g ${name}`;
      if (unit === 'Ml') return `${amount}ml ${name}`;
      if (unit === 'Tablespoon') return `${amount} colher(es) sopa ${name}`;
      if (unit === 'Teaspoon') return `${amount} colher(es) chá ${name}`;
      if (unit === 'Slice') return `${amount} fatia(s) ${name}`;
      if (unit === 'Units') return `${amount}x ${name}`;
      return `${amount} ${unit} ${name}`;
    });
  }
  if (option.description) {
    return option.description
      .split('\n')
      .map((line: string) => line.trim())
      .filter(Boolean);
  }
  return [];
}

function isOptionSelected(option: MealOption): boolean {
  return option.isSelected === true || option.selected === true;
}

interface EditFormState {
  name: string;
  calories: number;
  protein: number;
  carbs: number;
  fat: number;
}

interface NewIngredientState {
  name: string;
  amount: number;
  unit: string;
  category: string;
}

const emptyNewIngredient: NewIngredientState = { name: '', amount: 0, unit: 'Grams', category: 'Other' };

export function PlanPage() {
  const navigate = useNavigate();
  const [viewMode, setViewMode] = useState<ViewMode>('today');
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const setActiveMealPlanId = useUiStore((s) => s.setActiveMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const user = useAuthStore((s) => s.user);
  const [meals, setMeals] = useState<Meal[]>([]);
  const [loading, setLoading] = useState(false);

  // Auto-load active plan if localStorage was cleared
  useEffect(() => {
    if (!isAuthenticated || activePlanId) return;
    const loadLatestPlan = async () => {
      try {
        const { data } = await mealPlansApi.list();
        if (data?.length) {
          setActiveMealPlanId(data[0].id);
        }
      } catch { /* ignore */ }
    };
    loadLatestPlan();
  }, [isAuthenticated, activePlanId, setActiveMealPlanId]);
  const [selectingOption, setSelectingOption] = useState<string | null>(null);
  const [editingOptionId, setEditingOptionId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState<EditFormState>({ name: '', calories: 0, protein: 0, carbs: 0, fat: 0 });
  const [saving, setSaving] = useState(false);
  const [showAddIngredient, setShowAddIngredient] = useState(false);
  const [newIngredient, setNewIngredient] = useState<NewIngredientState>(emptyNewIngredient);

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
    if (!activePlanId || isOptionSelected(option) || editingOptionId) return;
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

  const startEditing = (option: MealOption) => {
    setEditingOptionId(option.id);
    setEditForm({
      name: option.name || '',
      calories: option.calories || 0,
      protein: option.proteinGrams ?? option.protein ?? 0,
      carbs: option.carbs ?? 0,
      fat: option.fat ?? 0,
    });
    setShowAddIngredient(false);
    setNewIngredient(emptyNewIngredient);
  };

  const cancelEditing = () => {
    setEditingOptionId(null);
    setShowAddIngredient(false);
    setNewIngredient(emptyNewIngredient);
  };

  const handleSaveOption = async () => {
    if (!activePlanId || !editingOptionId) return;
    setSaving(true);
    try {
      await mealPlansApi.updateOption(activePlanId, editingOptionId, {
        name: editForm.name,
        calories: editForm.calories,
        proteinGrams: editForm.protein,
        carbsGrams: editForm.carbs,
        fatGrams: editForm.fat,
      });
      setEditingOptionId(null);
      await fetchDay();
    } catch {
      // keep current state on error
    } finally {
      setSaving(false);
    }
  };

  const handleDeleteIngredient = async (ingredient: Ingredient) => {
    if (!activePlanId || !ingredient.id) return;
    try {
      await mealPlansApi.deleteIngredient(activePlanId, ingredient.id);
      await fetchDay();
    } catch {
      // keep current state on error
    }
  };

  const handleAddIngredient = async () => {
    if (!activePlanId || !editingOptionId || !newIngredient.name.trim()) return;
    setSaving(true);
    try {
      await mealPlansApi.addIngredient(activePlanId, editingOptionId, {
        name: newIngredient.name,
        amount: newIngredient.amount,
        unit: newIngredient.unit,
        category: newIngredient.category,
      });
      setNewIngredient(emptyNewIngredient);
      setShowAddIngredient(false);
      await fetchDay();
    } catch {
      // keep current state on error
    } finally {
      setSaving(false);
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

  const renderEditForm = (option: MealOption) => (
    <div className="space-y-4">
      {/* Option name */}
      <div>
        <label className="block text-xs font-label text-on-surface-variant mb-1">Option name</label>
        <input
          type="text"
          value={editForm.name}
          onChange={(e) => setEditForm((f) => ({ ...f, name: e.target.value }))}
          className="w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-3 py-2 text-sm text-on-surface focus:outline-none focus:border-primary"
        />
      </div>

      {/* Calories */}
      <div>
        <label className="block text-xs font-label text-on-surface-variant mb-1">Calories (kcal)</label>
        <input
          type="number"
          value={editForm.calories}
          onChange={(e) => setEditForm((f) => ({ ...f, calories: Number(e.target.value) }))}
          className="w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-3 py-2 text-sm text-on-surface focus:outline-none focus:border-primary"
        />
      </div>

      {/* Macros row */}
      <div className="grid grid-cols-3 gap-2">
        <div>
          <label className="block text-xs font-label text-on-surface-variant mb-1">Protein (g)</label>
          <input
            type="number"
            value={editForm.protein}
            onChange={(e) => setEditForm((f) => ({ ...f, protein: Number(e.target.value) }))}
            className="w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-3 py-2 text-sm text-on-surface focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-xs font-label text-on-surface-variant mb-1">Carbs (g)</label>
          <input
            type="number"
            value={editForm.carbs}
            onChange={(e) => setEditForm((f) => ({ ...f, carbs: Number(e.target.value) }))}
            className="w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-3 py-2 text-sm text-on-surface focus:outline-none focus:border-primary"
          />
        </div>
        <div>
          <label className="block text-xs font-label text-on-surface-variant mb-1">Fat (g)</label>
          <input
            type="number"
            value={editForm.fat}
            onChange={(e) => setEditForm((f) => ({ ...f, fat: Number(e.target.value) }))}
            className="w-full rounded-xl border border-outline-variant bg-surface-container-lowest px-3 py-2 text-sm text-on-surface focus:outline-none focus:border-primary"
          />
        </div>
      </div>

      {/* Ingredients */}
      <div>
        <p className="text-xs font-label font-medium text-on-surface-variant mb-2">Ingredients</p>
        <div className="space-y-1.5">
          {option.ingredients?.map((ing, i) => (
            <div key={ing.id || i} className="flex items-center gap-2 bg-surface-container rounded-xl px-3 py-2">
              <span className="flex-1 text-xs text-on-surface truncate">
                {ing.amount || ing.quantity} {ing.unit} {ing.namePt || ing.name}
              </span>
              <button
                type="button"
                onClick={() => handleDeleteIngredient(ing)}
                className="flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center text-on-surface-variant hover:bg-error-container hover:text-error transition-colors"
              >
                <Icon name="close" size={14} />
              </button>
            </div>
          ))}
        </div>

        {/* Add ingredient form */}
        {showAddIngredient ? (
          <div className="mt-3 space-y-2 bg-surface-container rounded-xl p-3">
            <input
              type="text"
              placeholder="Ingredient name"
              value={newIngredient.name}
              onChange={(e) => setNewIngredient((s) => ({ ...s, name: e.target.value }))}
              className="w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-3 py-1.5 text-xs text-on-surface focus:outline-none focus:border-primary"
            />
            <div className="grid grid-cols-3 gap-2">
              <input
                type="number"
                placeholder="Amount"
                value={newIngredient.amount || ''}
                onChange={(e) => setNewIngredient((s) => ({ ...s, amount: Number(e.target.value) }))}
                className="rounded-lg border border-outline-variant bg-surface-container-lowest px-3 py-1.5 text-xs text-on-surface focus:outline-none focus:border-primary"
              />
              <select
                value={newIngredient.unit}
                onChange={(e) => setNewIngredient((s) => ({ ...s, unit: e.target.value }))}
                className="rounded-lg border border-outline-variant bg-surface-container-lowest px-2 py-1.5 text-xs text-on-surface focus:outline-none focus:border-primary"
              >
                {UNIT_OPTIONS.map((u) => (
                  <option key={u} value={u}>{u}</option>
                ))}
              </select>
              <select
                value={newIngredient.category}
                onChange={(e) => setNewIngredient((s) => ({ ...s, category: e.target.value }))}
                className="rounded-lg border border-outline-variant bg-surface-container-lowest px-2 py-1.5 text-xs text-on-surface focus:outline-none focus:border-primary"
              >
                {CATEGORY_OPTIONS.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </select>
            </div>
            <div className="flex gap-2 pt-1">
              <button
                type="button"
                onClick={handleAddIngredient}
                disabled={saving || !newIngredient.name.trim()}
                className="flex items-center gap-1 bg-primary text-on-primary rounded-full px-3 py-1 text-xs font-medium disabled:opacity-50"
              >
                <Icon name="add" size={14} />
                Add
              </button>
              <button
                type="button"
                onClick={() => { setShowAddIngredient(false); setNewIngredient(emptyNewIngredient); }}
                className="text-on-surface-variant text-xs px-3 py-1"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <button
            type="button"
            onClick={() => setShowAddIngredient(true)}
            className="mt-2 flex items-center gap-1 text-primary text-xs font-medium hover:underline"
          >
            <Icon name="add" size={14} />
            Add ingredient
          </button>
        )}
      </div>

      {/* Save / Cancel */}
      <div className="flex gap-2 pt-1">
        <button
          type="button"
          onClick={handleSaveOption}
          disabled={saving}
          className="flex items-center gap-1.5 bg-primary text-on-primary rounded-full px-4 py-2 text-sm font-medium disabled:opacity-50"
        >
          <Icon name="save" size={16} />
          {saving ? 'Saving...' : 'Save'}
        </button>
        <button
          type="button"
          onClick={cancelEditing}
          className="flex items-center gap-1.5 text-on-surface-variant rounded-full px-4 py-2 text-sm font-medium hover:bg-surface-container"
        >
          <Icon name="close" size={16} />
          Cancel
        </button>
      </div>
    </div>
  );

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
            {[...meals].sort((a, b) => {
              const order: Record<string, number> = {
                Breakfast: 0, '0': 0,
                MorningSnack: 1, '1': 1,
                Lunch: 2, '2': 2,
                AfternoonSnack: 3, '3': 3,
                Dinner: 4, '4': 4,
                EveningSnack: 5, '5': 5,
                Snack: 6, '6': 6,
              };
              const aOrder = order[String(a.mealType)] ?? 99;
              const bOrder = order[String(b.mealType)] ?? 99;
              return aOrder - bOrder;
            }).map((meal) => {
              const mealLabel =
                MEALTYPE_LABELS[meal.mealType ?? 0] ||
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
                      const isEditing = editingOptionId === option.id;

                      if (isEditing) {
                        return (
                          <div
                            key={option.id}
                            className="rounded-2xl p-4 bg-surface-container-lowest border-2 border-primary editorial-shadow"
                          >
                            {renderEditForm(option)}
                          </div>
                        );
                      }

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
                              <div className="flex items-center gap-1">
                                <div
                                  role="button"
                                  tabIndex={0}
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    startEditing(option);
                                  }}
                                  onKeyDown={(e) => {
                                    if (e.key === 'Enter' || e.key === ' ') {
                                      e.stopPropagation();
                                      startEditing(option);
                                    }
                                  }}
                                  className="w-7 h-7 rounded-full flex items-center justify-center text-on-surface-variant hover:bg-surface-container transition-colors"
                                >
                                  <Icon name="edit" size={16} />
                                </div>
                                {selected && (
                                  <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center">
                                    <Icon name="check" size={14} className="text-on-primary" />
                                  </div>
                                )}
                              </div>
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
