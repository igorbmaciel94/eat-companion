import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { Button } from '../../../components/ui/Button';
import { useAuthStore } from '../../../stores/authStore';

export function ProfilePage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const logout = useAuthStore((s) => s.logout);

  const displayName = user?.displayName || 'User';
  const calorieTarget = user?.calorieTarget || 2000;
  const goalLabels: Record<number, string> = { 0: 'Lose Weight', 1: 'Eat Better', 2: 'Build Muscle' };
  const focusLabel = typeof user?.goalType === 'number' ? goalLabels[user.goalType] || 'Not set' : 'Not set';

  const handleLogout = () => {
    logout();
    navigate('/login');
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
            <p className="text-on-surface-variant text-sm">Member</p>
          </div>
          <span className="inline-flex items-center bg-primary-container text-on-primary-container text-xs font-label font-medium px-2.5 py-1 rounded-full">
            Active
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
            <p className="font-headline font-bold text-on-surface text-lg">—</p>
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
          <button className="w-full flex items-center justify-between p-4 text-left">
            <div>
              <p className="font-headline font-medium text-on-surface text-sm">
                Daily Calorie Target
              </p>
              <p className="text-on-surface-variant text-xs mt-0.5">{calorieTarget.toLocaleString()} kcal</p>
            </div>
            <Icon name="chevron_right" size={20} className="text-on-surface-variant" />
          </button>
          <button className="w-full flex items-center justify-between p-4 text-left">
            <div>
              <p className="font-headline font-medium text-on-surface text-sm">
                Dietary Patterns
              </p>
              <p className="text-on-surface-variant text-xs mt-0.5">Not set</p>
            </div>
            <Icon name="chevron_right" size={20} className="text-on-surface-variant" />
          </button>
          <button className="w-full flex items-center justify-between p-4 text-left">
            <div>
              <p className="font-headline font-medium text-on-surface text-sm">
                Notifications
              </p>
              <p className="text-on-surface-variant text-xs mt-0.5">Not configured</p>
            </div>
            <Icon name="chevron_right" size={20} className="text-on-surface-variant" />
          </button>
        </div>
      </div>

      {/* Logout */}
      <Button variant="danger" fullWidth size="lg" onClick={handleLogout}>
        Logout
      </Button>
    </div>
  );
}
