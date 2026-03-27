import { useState, useRef, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Icon } from '../../../components/ui/Icon';
import { ProgressBar } from '../../../components/ui/ProgressBar';
import { mealPlansApi } from '../../../api/mealPlans';
import { useUiStore } from '../../../stores/uiStore';

export function ImportPage() {
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [progress, setProgress] = useState(0);
  const [fileName, setFileName] = useState('');
  const [error, setError] = useState('');
  const [showConfirm, setShowConfirm] = useState(false);
  const [pendingFile, setPendingFile] = useState<File | null>(null);
  const pollIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const cleanupPolling = useCallback(() => {
    if (pollIntervalRef.current) {
      clearInterval(pollIntervalRef.current);
      pollIntervalRef.current = null;
    }
  }, []);

  useEffect(() => {
    return cleanupPolling;
  }, [cleanupPolling]);

  const startPolling = useCallback((id: string) => {
    cleanupPolling();
    // Save to store so global banner can track it too
    useUiStore.getState().setPendingImportJobId(id);
    let fakeProg = 30;

    pollIntervalRef.current = setInterval(async () => {
      try {
        const { data } = await mealPlansApi.getImportStatus(id);

        if (data.status === 'Processing') {
          fakeProg = Math.min(fakeProg + 5, 85);
          setProgress(fakeProg);
        }

        if (data.status === 'Completed' && data.mealPlanId) {
          cleanupPolling();
          setProgress(100);
          useUiStore.getState().setActiveMealPlanId(data.mealPlanId);
          useUiStore.getState().clearPendingImportJobId();
          setTimeout(() => navigate('/plan'), 600);
        }

        if (data.status === 'Failed') {
          cleanupPolling();
          useUiStore.getState().clearPendingImportJobId();
          setIsProcessing(false);
          setProgress(0);
          setError(data.errorMessage || 'Import failed. Please try again.');
        }
      } catch {
        // Keep polling on network errors
      }
    }, 3000);
  }, [cleanupPolling, navigate]);

  // Resume polling if returning to this page with a pending import
  useEffect(() => {
    const pendingJobId = useUiStore.getState().pendingImportJobId;
    if (pendingJobId && !isProcessing) {
      setIsProcessing(true);
      setProgress(30);
      setFileName('Resuming...');
      startPolling(pendingJobId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const startImport = async (file: File) => {
    setFileName(file.name);
    setIsProcessing(true);
    setProgress(10);
    setError('');
    setShowConfirm(false);

    try {
      const { data } = await mealPlansApi.import(file);
      setProgress(20);
      startPolling(data.jobId);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { status?: number; data?: { message?: string; detail?: string; jobId?: string } } };

      // If there's already a pending import, start polling it
      if (axiosErr.response?.status === 409 && axiosErr.response?.data?.jobId) {
        setProgress(20);
        startPolling(axiosErr.response.data.jobId);
        return;
      }

      setIsProcessing(false);
      setProgress(0);
      setError(axiosErr.response?.data?.detail || axiosErr.response?.data?.message || 'Failed to start import');
    }
  };

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const activePlanId = useUiStore.getState().activeMealPlanId;
    if (activePlanId) {
      setPendingFile(file);
      setShowConfirm(true);
      return;
    }

    startImport(file);
  };

  const handleConfirmReplace = () => {
    if (pendingFile) {
      startImport(pendingFile);
    }
  };

  const handleCancelReplace = () => {
    setShowConfirm(false);
    setPendingFile(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const statusText =
    progress < 20
      ? 'Uploading document...'
      : progress < 40
        ? 'Analyzing document structure...'
        : progress < 60
          ? 'Identifying portions and timings...'
          : progress < 85
            ? 'Mapping nutritional data...'
            : 'Finalizing your plan...';

  return (
    <div className="py-2">
      {/* Header */}
      <button
        onClick={() => navigate(-1)}
        className="flex items-center gap-1 text-on-surface-variant text-sm font-medium mb-4 -ml-1"
      >
        <Icon name="arrow_back" size={20} />
        Back
      </button>

      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-6">
        Import Plan
      </h1>

      {/* Confirmation dialog */}
      {showConfirm && (
        <div className="bg-error-container rounded-2xl p-5 editorial-shadow mb-6">
          <div className="flex items-start gap-3 mb-4">
            <Icon name="warning" size={24} className="text-on-error-container mt-0.5 flex-shrink-0" />
            <div>
              <p className="font-headline font-semibold text-on-error-container text-base mb-1">
                Replace existing plan?
              </p>
              <p className="text-on-error-container/80 text-sm">
                You already have an active meal plan. Importing a new one will replace it.
                Your meal logs and history will be preserved.
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <button
              onClick={handleConfirmReplace}
              className="flex-1 bg-error text-on-error rounded-full py-2.5 text-sm font-medium"
            >
              Replace Plan
            </button>
            <button
              onClick={handleCancelReplace}
              className="flex-1 bg-surface-container-lowest text-on-surface border border-outline rounded-full py-2.5 text-sm font-medium"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Upload area */}
      <div className="bg-surface-container-lowest rounded-2xl p-8 editorial-shadow mb-6">
        {!isProcessing ? (
          <div className="flex flex-col items-center text-center">
            <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-5">
              <Icon name="upload_file" size={36} className="text-primary" />
            </div>
            <h2 className="font-headline font-semibold text-on-surface text-xl mb-2">
              Upload Plan
            </h2>
            <p className="text-on-surface-variant text-sm mb-6 max-w-[240px]">
              Select a PDF document containing your nutrition or meal plan
            </p>
            {error && (
              <div className="text-error text-sm mb-4">{error}</div>
            )}
            <button
              onClick={() => fileInputRef.current?.click()}
              className="bg-primary text-on-primary rounded-full px-8 py-3 font-medium text-sm transition-colors hover:bg-primary-dim"
            >
              Select Document
            </button>
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf"
              onChange={handleFileChange}
              className="hidden"
            />
          </div>
        ) : (
          <div className="flex flex-col items-center text-center">
            <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-5">
              <Icon
                name="sync"
                size={36}
                className="text-primary animate-spin"
              />
            </div>
            <h2 className="font-headline font-semibold text-on-surface text-xl mb-1">
              Processing
            </h2>
            <p className="text-on-surface-variant text-sm mb-1">
              {fileName}
            </p>
            <p className="text-on-surface-variant text-xs mb-5">
              You can navigate away — processing continues in the background
            </p>

            <div className="w-full mb-2">
              <ProgressBar value={progress} max={100} height={6} />
            </div>
            <p className="text-on-surface-variant text-xs font-label mb-3">
              {progress}%
            </p>
            <p className="text-on-surface-variant text-sm italic">
              {statusText}
            </p>
          </div>
        )}
      </div>

      {/* Info card */}
      <div className="bg-primary-container/30 rounded-2xl p-5">
        <div className="flex items-start gap-3">
          <Icon name="auto_awesome" size={20} className="text-primary mt-0.5 flex-shrink-0" />
          <div>
            <p className="font-headline font-semibold text-on-surface text-sm mb-1">
              Smart Scanning
            </p>
            <p className="text-on-surface-variant text-xs leading-relaxed">
              Our AI analyzes your nutrition plan to extract meals, portions, and
              timing. Supported formats include PDF documents from nutritionists
              and dietitians.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
