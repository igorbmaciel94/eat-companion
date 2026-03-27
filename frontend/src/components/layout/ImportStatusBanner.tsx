import { useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../ui/Icon';
import { mealPlansApi } from '../../api/mealPlans';
import { useUiStore } from '../../stores/uiStore';

export function ImportStatusBanner() {
  const navigate = useNavigate();
  const pendingJobId = useUiStore((s) => s.pendingImportJobId);
  const setActiveMealPlanId = useUiStore((s) => s.setActiveMealPlanId);
  const clearPendingImportJobId = useUiStore((s) => s.clearPendingImportJobId);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const cleanup = useCallback(() => {
    if (pollRef.current) {
      clearInterval(pollRef.current);
      pollRef.current = null;
    }
  }, []);

  useEffect(() => {
    if (!pendingJobId) {
      cleanup();
      return;
    }

    // Start polling
    const poll = async () => {
      try {
        const { data } = await mealPlansApi.getImportStatus(pendingJobId);

        if (data.status === 'Completed' && data.mealPlanId) {
          cleanup();
          setActiveMealPlanId(data.mealPlanId);
          clearPendingImportJobId();
        }

        if (data.status === 'Failed') {
          cleanup();
          clearPendingImportJobId();
        }
      } catch {
        // Keep polling on network errors
      }
    };

    // Poll immediately, then every 3s
    poll();
    pollRef.current = setInterval(poll, 3000);

    return cleanup;
  }, [pendingJobId, cleanup, setActiveMealPlanId, clearPendingImportJobId]);

  if (!pendingJobId) return null;

  return (
    <div className="max-w-md mx-auto px-4">
      <button
        onClick={() => navigate('/plan/import')}
        className="w-full flex items-center gap-3 bg-primary-container rounded-2xl px-4 py-3 mb-2 transition-colors hover:bg-primary-container/80"
      >
        <Icon name="sync" size={20} className="text-primary animate-spin flex-shrink-0" />
        <div className="flex-1 text-left">
          <p className="text-on-primary-container text-sm font-medium">Import in progress...</p>
          <p className="text-on-primary-container/60 text-xs">Tap to see details</p>
        </div>
        <Icon name="chevron_right" size={20} className="text-on-primary-container/60" />
      </button>
    </div>
  );
}
