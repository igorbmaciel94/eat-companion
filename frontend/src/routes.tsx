import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';
import { AppShell } from './components/layout/AppShell';
import { AuthLayout } from './components/layout/AuthLayout';
import { OnboardingLayout } from './components/layout/OnboardingLayout';
import { LoadingSpinner } from './components/feedback/LoadingSpinner';

const LoginPage = lazy(() => import('./features/auth/pages/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage = lazy(() => import('./features/auth/pages/RegisterPage').then(m => ({ default: m.RegisterPage })));
const WelcomePage = lazy(() => import('./features/onboarding/pages/WelcomePage').then(m => ({ default: m.WelcomePage })));
const GoalSelectionPage = lazy(() => import('./features/onboarding/pages/GoalSelectionPage').then(m => ({ default: m.GoalSelectionPage })));
const TodayPage = lazy(() => import('./features/dashboard/pages/TodayPage').then(m => ({ default: m.TodayPage })));
const PlanPage = lazy(() => import('./features/plan/pages/PlanPage').then(m => ({ default: m.PlanPage })));
const ImportPage = lazy(() => import('./features/import/pages/ImportPage').then(m => ({ default: m.ImportPage })));
const GroceryListPage = lazy(() => import('./features/grocery/pages/GroceryListPage').then(m => ({ default: m.GroceryListPage })));
const ChartsPage = lazy(() => import('./features/charts/pages/ChartsPage').then(m => ({ default: m.ChartsPage })));
const ProfilePage = lazy(() => import('./features/profile/pages/ProfilePage').then(m => ({ default: m.ProfilePage })));

function SuspenseWrapper({ children }: { children: React.ReactNode }) {
  return (
    <Suspense fallback={<LoadingSpinner className="min-h-[50vh]" />}>
      {children}
    </Suspense>
  );
}

export function AppRoutes() {
  return (
    <Routes>
      {/* Auth routes */}
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<SuspenseWrapper><LoginPage /></SuspenseWrapper>} />
        <Route path="/register" element={<SuspenseWrapper><RegisterPage /></SuspenseWrapper>} />
      </Route>

      {/* Onboarding routes */}
      <Route element={<OnboardingLayout />}>
        <Route path="/welcome" element={<SuspenseWrapper><WelcomePage /></SuspenseWrapper>} />
        <Route path="/goals" element={<SuspenseWrapper><GoalSelectionPage /></SuspenseWrapper>} />
      </Route>

      {/* App routes with bottom nav */}
      <Route element={<AppShell />}>
        <Route path="/" element={<SuspenseWrapper><TodayPage /></SuspenseWrapper>} />
        <Route path="/plan" element={<SuspenseWrapper><PlanPage /></SuspenseWrapper>} />
        <Route path="/plan/import" element={<SuspenseWrapper><ImportPage /></SuspenseWrapper>} />
        <Route path="/grocery" element={<SuspenseWrapper><GroceryListPage /></SuspenseWrapper>} />
        <Route path="/charts" element={<SuspenseWrapper><ChartsPage /></SuspenseWrapper>} />
        <Route path="/profile" element={<SuspenseWrapper><ProfilePage /></SuspenseWrapper>} />
      </Route>
    </Routes>
  );
}
