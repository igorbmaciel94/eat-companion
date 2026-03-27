import { NavLink } from 'react-router-dom';
import { Icon } from '../ui/Icon';

const tabs = [
  { to: '/', label: 'Today', icon: 'calendar_today' },
  { to: '/plan', label: 'Plan', icon: 'restaurant_menu' },
  { to: '/grocery', label: 'Grocery', icon: 'shopping_basket' },
  { to: '/charts', label: 'Charts', icon: 'insights' },
  { to: '/profile', label: 'Profile', icon: 'person' },
];

export function BottomNavBar() {
  return (
    <nav className="fixed bottom-0 left-0 w-full flex justify-around items-center px-4 pb-6 pt-3 bg-white/80 backdrop-blur-xl z-50 rounded-t-[2rem] shadow-[0_-4px_24px_rgba(42,52,50,0.06)]">
      {tabs.map((tab) => (
        <NavLink
          key={tab.to}
          to={tab.to}
          end={tab.to === '/'}
          className={({ isActive }) =>
            `flex flex-col items-center gap-0.5 text-xs font-label transition-colors ${
              isActive ? 'text-primary' : 'text-on-surface-variant'
            }`
          }
        >
          {({ isActive }) => (
            <>
              <div
                className={`flex items-center justify-center w-14 h-8 rounded-full transition-colors ${
                  isActive ? 'bg-primary-container' : ''
                }`}
              >
                <Icon name={tab.icon} filled={isActive} size={22} />
              </div>
              <span className="font-medium">{tab.label}</span>
            </>
          )}
        </NavLink>
      ))}
    </nav>
  );
}
