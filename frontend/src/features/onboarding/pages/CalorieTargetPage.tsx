import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { Button } from '../../../components/ui/Button';
import { profileApi } from '../../../api/profile';
import { useAuthStore } from '../../../stores/authStore';

const PRESETS = [
  { label: 'Light (1 500 kcal)', value: 1500, icon: 'local_florist' },
  { label: 'Moderate (2 000 kcal)', value: 2000, icon: 'directions_walk' },
  { label: 'Active (2 500 kcal)', value: 2500, icon: 'directions_run' },
];

export function CalorieTargetPage() {
  const navigate = useNavigate();
  const setOnboardingCompleted = useAuthStore((s) => s.setOnboardingCompleted);
  const updateUser = useAuthStore((s) => s.updateUser);
  const [selected, setSelected] = useState<number>(2000);
  const [custom, setCustom] = useState('');
  const [useCustom, setUseCustom] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const target = useCustom ? parseInt(custom, 10) : selected;
    if (!target || target < 500 || target > 9999) return;

    setLoading(true);
    setError(null);
    try {
      const { data } = await profileApi.update({ calorieTarget: target, onboardingCompleted: true });
      updateUser({ calorieTarget: data.calorieTarget, onboardingCompleted: data.onboardingCompleted });
      setOnboardingCompleted();
      navigate('/plan/import');
    } catch {
      setError('Could not save. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col flex-1">
      {/* Step indicator */}
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-2">
        Step 3 of 3
      </p>

      {/* Heading */}
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-2">
        Daily calorie target
      </h1>
      <p className="text-on-surface-variant text-base mb-8">
        Your nutritionist's plan will be tracked against this target.
      </p>

      {error && (
        <p className="text-error text-sm mb-4">{error}</p>
      )}

      <form onSubmit={handleSubmit} className="flex flex-col flex-1">
        {/* Preset cards */}
        {!useCustom && (
          <div className="space-y-3 mb-6">
            {PRESETS.map((preset) => (
              <button
                key={preset.value}
                type="button"
                onClick={() => setSelected(preset.value)}
                className={`w-full flex items-center gap-4 rounded-[1rem] p-4 editorial-shadow text-left transition-all ${
                  selected === preset.value
                    ? 'bg-primary text-on-primary'
                    : 'bg-surface-container-lowest text-on-surface'
                }`}
              >
                <div className={`flex items-center justify-center w-10 h-10 rounded-full ${
                  selected === preset.value ? 'bg-on-primary/20' : 'bg-primary-container'
                }`}>
                  <Icon
                    name={preset.icon}
                    size={20}
                    className={selected === preset.value ? 'text-on-primary' : 'text-primary'}
                  />
                </div>
                <span className="font-medium text-sm">{preset.label}</span>
                {selected === preset.value && (
                  <Icon name="check_circle" size={20} className="text-on-primary ml-auto" />
                )}
              </button>
            ))}
          </div>
        )}

        {/* Custom input */}
        {useCustom && (
          <div className="mb-6">
            <div className="relative">
              <input
                type="number"
                min={500}
                max={9999}
                placeholder="e.g. 1800"
                value={custom}
                onChange={(e) => setCustom(e.target.value)}
                className="w-full rounded-[0.75rem] border border-outline-variant px-4 py-3 text-sm text-on-surface bg-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary pr-16"
                autoFocus
              />
              <span className="absolute right-4 top-1/2 -translate-y-1/2 text-on-surface-variant text-sm font-medium">
                kcal
              </span>
            </div>
          </div>
        )}

        {/* Toggle custom */}
        <button
          type="button"
          onClick={() => setUseCustom((v) => !v)}
          className="text-primary text-sm font-medium mb-8 text-left"
        >
          {useCustom ? '← Choose a preset' : 'Enter a custom target'}
        </button>

        <div className="flex-1" />

        <Button type="submit" fullWidth size="lg" disabled={loading || (useCustom && !custom)}>
          {loading ? 'Saving...' : 'Continue'}
        </Button>
      </form>
    </div>
  );
}
