import { Outlet } from 'react-router-dom';
import { TopAppBar } from './TopAppBar';
import { BottomNavBar } from './BottomNavBar';
import { ImportStatusBanner } from './ImportStatusBanner';

export function AppShell() {
  return (
    <div className="min-h-screen bg-surface">
      <TopAppBar />
      <div className="pt-16 pb-24">
        <ImportStatusBanner />
        <main className="max-w-md mx-auto px-4">
          <Outlet />
        </main>
      </div>
      <BottomNavBar />
    </div>
  );
}
