import { type InputHTMLAttributes, forwardRef } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, className = '', id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="block text-sm font-medium text-on-surface mb-1.5">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={`
            w-full rounded-[0.75rem] border px-4 py-2.5 text-sm text-on-surface
            bg-surface placeholder:text-on-surface-variant
            focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary
            ${error ? 'border-error' : 'border-outline-variant'}
            ${className}
          `}
          {...props}
        />
        {error && <p className="mt-1 text-xs text-error">{error}</p>}
        {helperText && !error && <p className="mt-1 text-xs text-on-surface-variant">{helperText}</p>}
      </div>
    );
  }
);

Input.displayName = 'Input';
