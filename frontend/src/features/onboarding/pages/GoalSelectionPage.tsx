import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';

const goals = [
  {
    icon: 'balance',
    title: 'Lose weight',
    subtitle: 'Calorie-controlled plans for sustainable weight loss',
  },
  {
    icon: 'fitness_center',
    title: 'Build muscle',
    subtitle: 'High-protein meals to support strength training',
  },
  {
    icon: 'eco',
    title: 'Eat better',
    subtitle: 'Balanced nutrition for overall well-being',
  },
];

export function GoalSelectionPage() {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col flex-1">
      {/* Step indicator */}
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-2">
        Step 1 of 3
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
            onClick={() => navigate('/')}
            className="w-full flex items-center gap-4 bg-surface-container-lowest rounded-[1rem] p-4 editorial-shadow text-left transition-transform active:scale-[0.98]"
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
            <Icon name="chevron_right" className="text-on-surface-variant" size={20} />
          </button>
        ))}
      </div>
    </div>
  );
}
