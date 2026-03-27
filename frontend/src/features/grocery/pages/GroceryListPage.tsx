import { Icon } from '../../../components/ui/Icon';
import { useGroceryList } from '../hooks/useGroceryList';

const categoryColors: Record<string, string> = {
  Produce: 'bg-primary',
  Protein: 'bg-error',
  Dairy: 'bg-tertiary',
  Grains: 'bg-secondary',
};

export function GroceryListPage() {
  const { data, checkedItems, toggleItem } = useGroceryList();

  if (data.totalItems === 0) {
    return (
      <div className="py-2 flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
        <div className="w-20 h-20 rounded-full bg-primary-container flex items-center justify-center mb-6">
          <Icon name="shopping_basket" size={40} className="text-primary" />
        </div>
        <h2 className="text-2xl font-headline font-bold text-on-surface mb-2">No grocery list yet</h2>
        <p className="text-on-surface-variant text-base mb-8">
          Import a meal plan first to generate your grocery list.
        </p>
      </div>
    );
  }

  return (
    <div className="py-2">
      {/* Header */}
      <p className="text-on-surface-variant font-label text-xs uppercase tracking-widest mb-1">
        Current Plan
      </p>
      <h1 className="text-3xl font-headline font-bold tracking-tight text-on-surface mb-1">
        Grocery List
      </h1>
      <p className="text-on-surface-variant text-sm mb-6">{data.dateRange}</p>

      {/* Stat cards */}
      <div className="grid grid-cols-2 gap-3 mb-6">
        <div className="bg-surface-container-lowest rounded-2xl p-4 editorial-shadow">
          <div className="flex items-center gap-2 mb-2">
            <Icon name="shopping_basket" size={20} className="text-primary" />
            <span className="text-on-surface-variant text-xs font-label uppercase tracking-widest">
              Total Items
            </span>
          </div>
          <p className="text-2xl font-headline font-bold text-on-surface">
            {data.totalItems}
          </p>
        </div>
        <div className="bg-surface-container-lowest rounded-2xl p-4 editorial-shadow">
          <div className="flex items-center gap-2 mb-2">
            <Icon name="timer" size={20} className="text-tertiary" />
            <span className="text-on-surface-variant text-xs font-label uppercase tracking-widest">
              Days
            </span>
          </div>
          <p className="text-2xl font-headline font-bold text-on-surface">
            {data.consolidationDays}
          </p>
        </div>
      </div>

      {/* Categories */}
      <div className="space-y-6">
        {data.categories.map((category) => (
          <div key={category.name}>
            {/* Category header */}
            <div className="flex items-center gap-2 mb-3">
              <div
                className={`w-1 h-5 rounded-full ${categoryColors[category.name] ?? 'bg-primary'}`}
              />
              <h2 className="font-headline font-semibold text-on-surface text-base">
                {category.name}
              </h2>
              <span className="text-on-surface-variant text-xs font-label">
                {category.items.length} items
              </span>
            </div>

            {/* Item list */}
            <div className="space-y-0">
              {category.items.map((item) => {
                const isChecked = checkedItems.has(item.name);
                return (
                  <button
                    key={item.id}
                    onClick={() => toggleItem(item.name, item.id)}
                    className="w-full flex items-center gap-3 py-2.5 px-1 text-left transition-colors"
                  >
                    <div
                      className={`w-5 h-5 rounded flex-shrink-0 flex items-center justify-center border transition-colors ${
                        isChecked
                          ? 'bg-primary border-primary'
                          : 'border-outline-variant bg-transparent'
                      }`}
                    >
                      {isChecked && (
                        <Icon name="check" size={14} className="text-on-primary" />
                      )}
                    </div>
                    <span
                      className={`text-sm transition-all ${
                        isChecked
                          ? 'line-through text-on-surface-variant'
                          : 'text-on-surface'
                      }`}
                    >
                      {item.name}
                    </span>
                  </button>
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
