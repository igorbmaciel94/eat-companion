import { useState, useEffect, useCallback } from 'react';
import { groceryListsApi } from '../../../api/groceryLists';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';

interface Category {
  name: string;
  items: { id: string; name: string }[];
}

interface GroceryListData {
  totalItems: number;
  consolidationDays: number;
  dateRange: string;
  categories: Category[];
  listId?: string;
}

const mockData: GroceryListData = {
  totalItems: 24,
  consolidationDays: 3,
  dateRange: 'Oct 24 — Oct 26',
  categories: [
    {
      name: 'Produce',
      items: [
        { id: 'p1', name: 'Organic Baby Spinach' },
        { id: 'p2', name: 'Avocados' },
        { id: 'p3', name: 'Blueberries' },
        { id: 'p4', name: 'Mixed Greens' },
        { id: 'p5', name: 'Tomatoes' },
        { id: 'p6', name: 'Lemons' },
        { id: 'p7', name: 'Bananas' },
        { id: 'p8', name: 'Berries' },
      ],
    },
    {
      name: 'Protein',
      items: [
        { id: 'pr1', name: 'Chicken Breast' },
        { id: 'pr2', name: 'Salmon Fillet' },
        { id: 'pr3', name: 'Eggs (dozen)' },
        { id: 'pr4', name: 'Greek Yogurt' },
      ],
    },
    {
      name: 'Dairy',
      items: [
        { id: 'd1', name: 'Edam Cheese' },
        { id: 'd2', name: 'Cottage Cheese' },
        { id: 'd3', name: 'Protein Yogurt' },
        { id: 'd4', name: 'Low-fat Milk' },
      ],
    },
    {
      name: 'Grains',
      items: [
        { id: 'g1', name: 'Bread (whole grain)' },
        { id: 'g2', name: 'Rice' },
        { id: 'g3', name: 'Oat Flakes' },
        { id: 'g4', name: 'Pasta' },
        { id: 'g5', name: 'Corn Cakes' },
      ],
    },
  ],
};

export function useGroceryList() {
  const [checkedItems, setCheckedItems] = useState<Set<string>>(new Set());
  const [data, setData] = useState<GroceryListData>(mockData);
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  useEffect(() => {
    if (!isAuthenticated || !activePlanId) return;

    const fetchGroceryList = async () => {
      try {
        // Generate a grocery list for the next 3 days
        const today = new Date();
        const startDate = today.toISOString().split('T')[0];
        const endDate = new Date(today.getTime() + 2 * 86400000).toISOString().split('T')[0];
        const { data: listData } = await groceryListsApi.generate(activePlanId, startDate, endDate);

        if (listData && listData.id) {
          // Fetch the full list
          const { data: fullList } = await groceryListsApi.getById(listData.id);

          if (fullList && fullList.items && fullList.items.length > 0) {
            // Group items by category
            const categoryMap = new Map<string, { id: string; name: string }[]>();
            const checked = new Set<string>();

            for (const item of fullList.items) {
              const cat = item.category || 'Other';
              if (!categoryMap.has(cat)) categoryMap.set(cat, []);
              categoryMap.get(cat)!.push({ id: item.id, name: item.name });
              if (item.isChecked || item.checked) checked.add(item.name);
            }

            const categories: Category[] = Array.from(categoryMap.entries()).map(([name, items]) => ({
              name,
              items,
            }));

            setData({
              totalItems: fullList.items.length,
              consolidationDays: 3,
              dateRange: `${formatShortDate(startDate)} — ${formatShortDate(endDate)}`,
              categories,
              listId: fullList.id,
            });
            setCheckedItems(checked);
          }
        }
      } catch {
        // Keep mock data on error
      }
    };

    fetchGroceryList();
  }, [activePlanId, isAuthenticated]);

  const toggleItem = useCallback(async (itemName: string, itemId?: string) => {
    setCheckedItems((prev) => {
      const next = new Set(prev);
      if (next.has(itemName)) {
        next.delete(itemName);
      } else {
        next.add(itemName);
      }
      return next;
    });

    // Toggle on backend if we have a listId
    if (isAuthenticated && data.listId && itemId) {
      try {
        await groceryListsApi.toggleItem(data.listId, itemId);
      } catch { /* ignore */ }
    }
  }, [isAuthenticated, data.listId]);

  const checkedCount = checkedItems.size;
  const progress = data.totalItems > 0 ? (checkedCount / data.totalItems) * 100 : 0;

  return {
    data,
    checkedItems,
    toggleItem,
    checkedCount,
    progress,
  };
}

function formatShortDate(dateStr: string): string {
  const d = new Date(dateStr + 'T00:00:00');
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}
