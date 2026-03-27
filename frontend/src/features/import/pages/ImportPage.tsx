import { useState, useRef } from 'react';
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

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setFileName(file.name);
    setIsProcessing(true);
    setProgress(10);
    setError('');

    try {
      setProgress(30);
      const { data } = await mealPlansApi.import(file);
      setProgress(80);

      // Store the active meal plan
      useUiStore.getState().setActiveMealPlanId(data.mealPlanId);

      setProgress(100);
      // Navigate after brief delay
      setTimeout(() => navigate('/plan'), 600);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { detail?: string } } };
      setIsProcessing(false);
      setProgress(0);
      setError(axiosErr.response?.data?.detail || 'Failed to import PDF');
    }
  };

  const statusText =
    progress < 30
      ? 'Analyzing document structure...'
      : progress < 60
        ? 'Identifying portions and timings...'
        : progress < 90
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

      {/* Upload area */}
      <div className="bg-surface-container-lowest rounded-2xl p-8 editorial-shadow mb-6">
        {!isProcessing ? (
          /* Upload state */
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
          /* Processing state */
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
            <p className="text-on-surface-variant text-sm mb-5">
              {fileName}
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
