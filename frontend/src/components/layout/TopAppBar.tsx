import { Icon } from '../ui/Icon';

export function TopAppBar() {
  return (
    <header className="bg-surface/80 backdrop-blur-md fixed top-0 w-full z-50 shadow-sm shadow-on-surface/5">
      <div className="max-w-md mx-auto flex items-center justify-between px-4 py-3">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-primary-container flex items-center justify-center">
            <span className="text-xs font-semibold text-on-primary-container">A</span>
          </div>
          <h1 className="font-headline font-bold text-lg text-primary">The Atrium</h1>
        </div>
        <button className="p-2 rounded-full hover:bg-surface-container transition-colors">
          <Icon name="notifications" size={22} className="text-on-surface-variant" />
        </button>
      </div>
    </header>
  );
}
