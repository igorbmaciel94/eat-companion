interface ProgressBarProps {
  value: number;
  max?: number;
  height?: number;
  className?: string;
}

export function ProgressBar({ value, max = 100, height = 8, className = '' }: ProgressBarProps) {
  const percentage = Math.min(Math.max((value / max) * 100, 0), 100);

  return (
    <div
      className={`w-full rounded-full bg-surface-container-high overflow-hidden ${className}`}
      style={{ height }}
    >
      <div
        className="h-full rounded-full bg-primary transition-all duration-300 ease-out"
        style={{ width: `${percentage}%` }}
      />
    </div>
  );
}
