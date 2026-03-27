import { Outlet } from 'react-router-dom';
import { TopAppBar } from './TopAppBar';
import { BottomNavBar } from './BottomNavBar';

export function AppShell() {
  return (
    <div className="min-h-screen bg-surface">
      <TopAppBar />
      <main className="max-w-md mx-auto px-4 pt-16 pb-24">
        <Outlet />
      </main>
      <BottomNavBar />
    </div>
  );
}
