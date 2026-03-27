import { Outlet } from 'react-router-dom';

export function AuthLayout() {
  return (
    <div className="min-h-screen bg-surface flex flex-col items-center justify-center px-6">
      <div className="w-full max-w-sm">
        <div className="text-center mb-8">
          <h1 className="font-headline font-bold text-2xl text-primary">Eat Companion</h1>
          <p className="text-sm text-on-surface-variant mt-1">Your Meal Plan Partner</p>
        </div>
        <Outlet />
      </div>
    </div>
  );
}
