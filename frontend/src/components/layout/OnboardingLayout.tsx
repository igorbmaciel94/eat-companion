import { Outlet } from 'react-router-dom';

export function OnboardingLayout() {
  return (
    <div className="min-h-screen bg-surface flex flex-col px-6 py-12">
      <div className="w-full max-w-md mx-auto flex-1 flex flex-col">
        <Outlet />
      </div>
    </div>
  );
}
