import type { HTMLAttributes, ReactNode } from 'react';

type CardVariant = 'elevated' | 'filled' | 'outlined';

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  variant?: CardVariant;
  padding?: 'none' | 'sm' | 'md' | 'lg';
  children: ReactNode;
}

const variantClasses: Record<CardVariant, string> = {
  elevated: 'bg-white editorial-shadow',
  filled: 'bg-surface-container',
  outlined: 'border border-outline-variant bg-surface',
};

const paddingClasses: Record<string, string> = {
  none: '',
  sm: 'p-3',
  md: 'p-4',
  lg: 'p-6',
};

export function Card({ variant = 'elevated', padding = 'md', className = '', children, ...props }: CardProps) {
  return (
    <div
      className={`rounded-[1rem] ${variantClasses[variant]} ${paddingClasses[padding]} ${className}`}
      {...props}
    >
      {children}
    </div>
  );
}
