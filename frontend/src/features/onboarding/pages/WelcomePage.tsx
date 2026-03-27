import { useNavigate } from 'react-router-dom';
import { Button } from '../../../components/ui/Button';

export function WelcomePage() {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col flex-1">
      {/* Hero gradient placeholder */}
      <div className="relative w-full h-56 rounded-2xl overflow-hidden mb-8 bg-gradient-to-br from-primary/20 via-primary-container to-tertiary-container">
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="material-symbols-outlined text-primary/40" style={{ fontSize: 96 }}>
            spa
          </span>
        </div>
      </div>

      {/* Badge */}
      <div className="mb-4">
        <span className="inline-block bg-primary-container text-on-primary-container text-xs font-label font-medium px-3 py-1 rounded-full">
          Mindful Beginnings
        </span>
      </div>

      {/* Headline */}
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-3">
        Welcome to your{' '}
        <span className="italic text-primary">mindful kitchen.</span>
      </h1>

      {/* Subtitle */}
      <p className="text-on-surface-variant text-base leading-relaxed mb-8">
        Build healthy habits with personalized meal plans, smart grocery lists, and intuitive progress tracking.
      </p>

      {/* Spacer */}
      <div className="flex-1" />

      {/* CTA */}
      <div className="pb-8">
        <Button fullWidth size="lg" onClick={() => navigate('/goals')}>
          Get Started
        </Button>
      </div>
    </div>
  );
}
