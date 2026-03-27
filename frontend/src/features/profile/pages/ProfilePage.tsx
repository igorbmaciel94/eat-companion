import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { Button } from '../../../components/ui/Button';
import { useAuthStore } from '../../../stores/authStore';
import { profileApi } from '../../../api/profile';

const GOAL_OPTIONS = [
  { value: 0, label: 'Lose Weight', icon: 'monitor_weight' },
  { value: 1, label: 'Eat Better', icon: 'restaurant' },
  { value: 2, label: 'Build Muscle', icon: 'fitness_center' },
] as const;

const CALORIE_PRESETS = [1500, 2000, 2500];

export function ProfilePage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const updateUser = useAuthStore((s) => s.updateUser);

  const [editingCalories, setEditingCalories] = useState(false);
  const [editingGoal, setEditingGoal] = useState(false);
  const [customCalories, setCustomCalories] = useState('');
  const [saving, setSaving] = useState(false);

  const displayName = user?.displayName || 'User';
  const email = user?.email;
  const calorieTarget = user?.calorieTarget || 2000;
  const goalLabels: Record<number, string> = { 0: 'Lose Weight', 1: 'Eat Better', 2: 'Build Muscle' };
  const focusLabel = typeof user?.goalType === 'number' ? goalLabels[user.goalType] || 'Not set' : 'Not set';

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleSaveCalories = async (value: number) => {
    if (value < 500 || value > 10000 || saving) return;
    setSaving(true);
    try {
      await profileApi.update({ calorieTarget: value });
      updateUser({ calorieTarget: value });
      setEditingCalories(false);
      setCustomCalories('');
    } finally {
      setSaving(false);
    }
  };

  const handleSaveGoal = async (goalType: number) => {
    if (saving || goalType === user?.goalType) return;
    setSaving(true);
    try {
      await profileApi.update({ goalType });
      updateUser({ goalType });
      setEditingGoal(false);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="py-2">
      {/* User info card */}
      <div className="bg-surface-container-lowest rounded-2xl p-5 editorial-shadow mb-6">
        <div className="flex items-center gap-4 mb-5">
          {/* Avatar */}
          <div className="w-16 h-16 rounded-full bg-primary-container flex items-center justify-center flex-shrink-0">
            <Icon name="person" size={32} className="text-primary" />
          </div>
          <div className="flex-1 min-w-0">
            <h1 className="font-headline font-bold text-on-surface text-xl">
              {displayName}
            </h1>
            {email && (
              <p className="text-on-surface-variant text-xs mt-0.5 truncate">{email}</p>
            )}
          </div>
          <span className="inline-flex items-center bg-primary-container text-on-primary-container text-xs font-label font-medium px-2.5 py-1 rounded-full">
            Member
          </span>
        </div>

        {/* Stat boxes */}
        <div className="grid grid-cols-2 gap-3">
          <div className="bg-surface-container-low rounded-xl p-3 text-center">
            <div className="flex items-center justify-center gap-1 mb-1">
              <Icon name="local_fire_department" size={16} className="text-error" />
              <span className="text-on-surface-variant text-[10px] font-label uppercase tracking-widest">
                Streak
              </span>
            </div>
            <p className="font-headline font-bold text-on-surface text-lg">&mdash;</p>
          </div>
          <div className="bg-surface-container-low rounded-xl p-3 text-center">
            <div className="flex items-center justify-center gap-1 mb-1">
              <Icon name="self_improvement" size={16} className="text-primary" />
              <span className="text-on-surface-variant text-[10px] font-label uppercase tracking-widest">
                Focus
              </span>
            </div>
            <p className="font-headline font-bold text-on-surface text-lg leading-tight">
              {focusLabel}
            </p>
          </div>
        </div>
      </div>

      {/* Settings section */}
      <div className="mb-6">
        <h2 className="text-[10px] font-label font-medium uppercase tracking-widest text-on-surface-variant mb-3">
          Settings
        </h2>

        <div className="bg-surface-container-lowest rounded-2xl editorial-shadow divide-y divide-outline-variant/30">
          {/* Daily Calorie Target */}
          <div>
            <button
              className="w-full flex items-center justify-between p-4 text-left"
              onClick={() => {
                setEditingCalories(!editingCalories);
                setEditingGoal(false);
              }}
            >
              <div>
                <p className="font-headline font-medium text-on-surface text-sm">
                  Daily Calorie Target
                </p>
                <p className="text-on-surface-variant text-xs mt-0.5">
                  {calorieTarget.toLocaleString()} kcal
                </p>
              </div>
              <Icon
                name={editingCalories ? 'expand_less' : 'chevron_right'}
                size={20}
                className="text-on-surface-variant"
              />
            </button>

            {editingCalories && (
              <div className="px-4 pb-4 space-y-3">
                <div className="flex gap-2">
                  {CALORIE_PRESETS.map((preset) => (
                    <button
                      key={preset}
                      disabled={saving}
                      onClick={() => handleSaveCalories(preset)}
                      className={`flex-1 py-2.5 rounded-xl text-sm font-label font-medium transition-colors ${
                        calorieTarget === preset
                          ? 'bg-primary text-on-primary'
                          : 'bg-surface-container-low text-on-surface hover:bg-surface-container'
                      } disabled:opacity-50`}
                    >
                      {preset.toLocaleString()}
                    </button>
                  ))}
                </div>

                <div className="flex gap-2">
                  <input
                    type="number"
                    inputMode="numeric"
                    placeholder="Custom (500–10000)"
                    value={customCalories}
                    onChange={(e) => setCustomCalories(e.target.value)}
                    className="flex-1 bg-surface-container-low rounded-xl px-3 py-2.5 text-sm text-on-surface placeholder:text-on-surface-variant/50 outline-none focus:ring-2 focus:ring-primary/30"
                  />
                  <button
                    disabled={saving || !customCalories || Number(customCalories) < 500 || Number(customCalories) > 10000}
                    onClick={() => handleSaveCalories(Number(customCalories))}
                    className="bg-primary text-on-primary px-4 py-2.5 rounded-xl text-sm font-label font-medium disabled:opacity-50 transition-colors"
                  >
                    Save
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Goal / Focus */}
          <div>
            <button
              className="w-full flex items-center justify-between p-4 text-left"
              onClick={() => {
                setEditingGoal(!editingGoal);
                setEditingCalories(false);
              }}
            >
              <div>
                <p className="font-headline font-medium text-on-surface text-sm">
                  Goal / Focus
                </p>
                <p className="text-on-surface-variant text-xs mt-0.5">{focusLabel}</p>
              </div>
              <Icon
                name={editingGoal ? 'expand_less' : 'chevron_right'}
                size={20}
                className="text-on-surface-variant"
              />
            </button>

            {editingGoal && (
              <div className="px-4 pb-4 space-y-2">
                {GOAL_OPTIONS.map((option) => {
                  const isSelected = user?.goalType === option.value;
                  return (
                    <button
                      key={option.value}
                      disabled={saving}
                      onClick={() => handleSaveGoal(option.value)}
                      className={`w-full flex items-center gap-3 p-3 rounded-xl transition-colors ${
                        isSelected
                          ? 'bg-primary-container text-on-primary-container'
                          : 'bg-surface-container-low text-on-surface hover:bg-surface-container'
                      } disabled:opacity-50`}
                    >
                      <Icon name={option.icon} size={20} />
                      <span className="text-sm font-label font-medium">{option.label}</span>
                      {isSelected && (
                        <Icon name="check" size={18} className="ml-auto text-primary" />
                      )}
                    </button>
                  );
                })}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Logout */}
      <Button variant="danger" fullWidth size="lg" onClick={handleLogout}>
        Logout
      </Button>
    </div>
  );
}
