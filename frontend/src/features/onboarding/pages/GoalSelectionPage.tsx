import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { profileApi } from '../../../api/profile';
import { useAuthStore } from '../../../stores/authStore';

const goals = [
  {
    icon: 'balance',
    title: 'Lose weight',
    subtitle: 'Calorie-controlled plans for sustainable weight loss',
    goalType: 0,
  },
  {
    icon: 'fitness_center',
    title: 'Build muscle',
    subtitle: 'High-protein meals to support strength training',
    goalType: 2,
  },
  {
    icon: 'eco',
    title: 'Eat better',
    subtitle: 'Balanced nutrition for overall well-being',
    goalType: 1,
  },
];

export function GoalSelectionPage() {
  const navigate = useNavigate();
  const setOnboardingCompleted = useAuthStore((s) => s.setOnboardingCompleted);
  const updateUser = useAuthStore((s) => s.updateUser);
  const [loading, setLoading] = useState<number | null>(null);

  const handleSelect = async (goalType: number) => {
    setLoading(goalType);
    try {
      const { data } = await profileApi.update({ goalType });
      updateUser({ goalType: data.goalType });
      setOnboardingCompleted();
      navigate('/');
    } catch {
      // Navigate anyway — goal can be updated later in profile
      setOnboardingCompleted();
      navigate('/');
    } finally {
      setLoading(null);
    }
  };

  return (
    <div className="flex flex-col flex-1">
      {/* Step indicator */}
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-2">
        Step 1 of 1
      </p>

      {/* Heading */}
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-8">
        What is your focus?
      </h1>

      {/* Goal cards */}
      <div className="space-y-3">
        {goals.map((goal) => (
          <button
            key={goal.title}
            onClick={() => handleSelect(goal.goalType)}
            disabled={loading !== null}
            className="w-full flex items-center gap-4 bg-surface-container-lowest rounded-[1rem] p-4 editorial-shadow text-left transition-transform active:scale-[0.98] disabled:opacity-60"
          >
            <div className="flex items-center justify-center w-12 h-12 rounded-full bg-primary-container">
              <Icon name={goal.icon} className="text-primary" size={24} />
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-headline font-semibold text-on-surface text-base">
                {goal.title}
              </p>
              <p className="text-on-surface-variant text-sm mt-0.5">
                {goal.subtitle}
              </p>
            </div>
            {loading === goal.goalType ? (
              <Icon name="progress_activity" className="text-primary animate-spin" size={20} />
            ) : (
              <Icon name="chevron_right" className="text-on-surface-variant" size={20} />
            )}
          </button>
        ))}
      </div>
    </div>
  );
}
