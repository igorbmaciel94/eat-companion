import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { Button } from '../../../components/ui/Button';
import { profileApi } from '../../../api/profile';
import { useAuthStore } from '../../../stores/authStore';

export function BodyMeasurementsPage() {
  const navigate = useNavigate();
  const updateUser = useAuthStore((s) => s.updateUser);
  const [height, setHeight] = useState('');
  const [weight, setWeight] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const heightVal = parseFloat(height.replace(',', '.'));
    const weightVal = parseFloat(weight.replace(',', '.'));

    setLoading(true);
    try {
      const updates: { heightCm?: number; weightKg?: number } = {};
      if (heightVal > 0) updates.heightCm = heightVal;
      if (weightVal > 0) updates.weightKg = weightVal;

      if (Object.keys(updates).length > 0) {
        const { data } = await profileApi.update(updates);
        updateUser({ heightCm: data.heightCm, weightKg: data.weightKg });
      }
    } catch {
      // proceed anyway
    } finally {
      setLoading(false);
      navigate('/onboarding/calories');
    }
  };

  return (
    <div className="flex flex-col flex-1">
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-2">
        Step 2 of 3
      </p>

      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-2">
        Your measurements
      </h1>
      <p className="text-on-surface-variant text-base mb-8">
        Help us track your progress. You can update these anytime in your profile.
      </p>

      <form onSubmit={handleSubmit} className="flex flex-col flex-1">
        <div className="space-y-4 mb-8">
          <div>
            <label className="block text-sm font-medium text-on-surface mb-1.5">
              Height
            </label>
            <div className="relative">
              <div className="absolute left-3 top-1/2 -translate-y-1/2">
                <Icon name="straighten" size={20} className="text-on-surface-variant" />
              </div>
              <input
                type="text"
                inputMode="decimal"
                pattern="[0-9]*[.,]?[0-9]*"
                placeholder="e.g. 175"
                value={height}
                onChange={(e) => setHeight(e.target.value)}
                className="w-full rounded-[0.75rem] border border-outline-variant pl-10 pr-12 py-3 text-sm text-on-surface bg-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
              />
              <span className="absolute right-4 top-1/2 -translate-y-1/2 text-on-surface-variant text-sm font-medium">
                cm
              </span>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-on-surface mb-1.5">
              Current weight
            </label>
            <div className="relative">
              <div className="absolute left-3 top-1/2 -translate-y-1/2">
                <Icon name="monitor_weight" size={20} className="text-on-surface-variant" />
              </div>
              <input
                type="text"
                inputMode="decimal"
                pattern="[0-9]*[.,]?[0-9]*"
                placeholder="e.g. 75,5"
                value={weight}
                onChange={(e) => setWeight(e.target.value)}
                className="w-full rounded-[0.75rem] border border-outline-variant pl-10 pr-12 py-3 text-sm text-on-surface bg-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
              />
              <span className="absolute right-4 top-1/2 -translate-y-1/2 text-on-surface-variant text-sm font-medium">
                kg
              </span>
            </div>
          </div>
        </div>

        <div className="flex-1" />

        <div className="space-y-3">
          <Button type="submit" fullWidth size="lg" disabled={loading}>
            {loading ? 'Saving...' : 'Continue'}
          </Button>
          <button
            type="button"
            onClick={() => navigate('/onboarding/calories')}
            className="w-full text-center text-primary text-sm font-medium py-2"
          >
            Skip for now
          </button>
        </div>
      </form>
    </div>
  );
}
