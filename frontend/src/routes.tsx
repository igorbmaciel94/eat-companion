import { lazy, Suspense } from 'react';
import { Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AppShell } from './components/layout/AppShell';
import { AuthLayout } from './components/layout/AuthLayout';
import { OnboardingLayout } from './components/layout/OnboardingLayout';
import { LoadingSpinner } from './components/feedback/LoadingSpinner';
import { useAuthStore } from './stores/authStore';

const LoginPage = lazy(() => import('./features/auth/pages/LoginPage').then(m => ({ default: m.LoginPage })));
const RegisterPage = lazy(() => import('./features/auth/pages/RegisterPage').then(m => ({ default: m.RegisterPage })));
const WelcomePage = lazy(() => import('./features/onboarding/pages/WelcomePage').then(m => ({ default: m.WelcomePage })));
const GoalSelectionPage = lazy(() => import('./features/onboarding/pages/GoalSelectionPage').then(m => ({ default: m.GoalSelectionPage })));
const BodyMeasurementsPage = lazy(() => import('./features/onboarding/pages/BodyMeasurementsPage').then(m => ({ default: m.BodyMeasurementsPage })));
const CalorieTargetPage = lazy(() => import('./features/onboarding/pages/CalorieTargetPage').then(m => ({ default: m.CalorieTargetPage })));
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

/** Redirects to /login if user is not authenticated */
function RequireAuth() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <Outlet />;
}

/** Redirects authenticated users: to /welcome if onboarding pending, to / if completed */
function RedirectIfAuth() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const onboardingCompleted = useAuthStore((s) => s.onboardingCompleted);
  if (isAuthenticated) return <Navigate to={onboardingCompleted ? '/' : '/welcome'} replace />;
  return <Outlet />;
}

/** Redirects to /welcome if onboarding not completed */
function RequireOnboardingComplete() {
  const onboardingCompleted = useAuthStore((s) => s.onboardingCompleted);
  if (!onboardingCompleted) return <Navigate to="/welcome" replace />;
  return <Outlet />;
}

/** Redirects to / if user already completed onboarding */
function RequireOnboarding() {
  const onboardingCompleted = useAuthStore((s) => s.onboardingCompleted);
  if (onboardingCompleted) return <Navigate to="/" replace />;
  return <Outlet />;
}

export function AppRoutes() {
  return (
    <Routes>
      {/* Public auth routes — redirect to app if already logged in */}
      <Route element={<RedirectIfAuth />}>
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<SuspenseWrapper><LoginPage /></SuspenseWrapper>} />
          <Route path="/register" element={<SuspenseWrapper><RegisterPage /></SuspenseWrapper>} />
        </Route>
      </Route>

      {/* Protected routes — redirect to /login if not authenticated */}
      <Route element={<RequireAuth />}>
        {/* Onboarding — only accessible if NOT completed */}
        <Route element={<RequireOnboarding />}>
          <Route element={<OnboardingLayout />}>
            <Route path="/welcome" element={<SuspenseWrapper><WelcomePage /></SuspenseWrapper>} />
            <Route path="/goals" element={<SuspenseWrapper><GoalSelectionPage /></SuspenseWrapper>} />
            <Route path="/onboarding/body" element={<SuspenseWrapper><BodyMeasurementsPage /></SuspenseWrapper>} />
            <Route path="/onboarding/calories" element={<SuspenseWrapper><CalorieTargetPage /></SuspenseWrapper>} />
          </Route>
        </Route>

        {/* Import — accessible anytime (needed right after onboarding) */}
        <Route element={<AppShell />}>
          <Route path="/plan/import" element={<SuspenseWrapper><ImportPage /></SuspenseWrapper>} />
        </Route>

        {/* Main app — only accessible AFTER onboarding completed */}
        <Route element={<RequireOnboardingComplete />}>
          <Route element={<AppShell />}>
            <Route path="/" element={<SuspenseWrapper><TodayPage /></SuspenseWrapper>} />
            <Route path="/plan" element={<SuspenseWrapper><PlanPage /></SuspenseWrapper>} />
            <Route path="/grocery" element={<SuspenseWrapper><GroceryListPage /></SuspenseWrapper>} />
            <Route path="/charts" element={<SuspenseWrapper><ChartsPage /></SuspenseWrapper>} />
            <Route path="/profile" element={<SuspenseWrapper><ProfilePage /></SuspenseWrapper>} />
          </Route>
        </Route>
      </Route>

      {/* Catch-all: redirect to login */}
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
