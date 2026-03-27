import { useState, useCallback, useEffect } from 'react';
import { groceryListsApi } from '../../../api/groceryLists';
import { useUiStore } from '../../../stores/uiStore';
import { useAuthStore } from '../../../stores/authStore';

interface GroceryItemView {
  id: string;
  name: string;
  namePt?: string;
  totalAmount?: number;
  unit?: string;
}

interface Category {
  name: string;
  items: GroceryItemView[];
}

interface GroceryListData {
  totalItems: number;
  consolidationDays: number;
  dateRange: string;
  categories: Category[];
  listId?: string;
}

const PT_UNIT_LABELS: Record<string, string> = {
  Grams: 'g',
  Ml: 'ml',
  Tablespoon: ' colheres sopa',
  Teaspoon: ' colheres chá',
  Slice: ' fatias',
  Units: ' unidades',
};

const emptyData: GroceryListData = {
  totalItems: 0,
  consolidationDays: 0,
  dateRange: '',
  categories: [],
};

function parseGroceryList(fullList: { id: string; startDate: string; endDate: string; items: { id: string; name: string; namePt?: string; totalAmount?: number; unit?: string; category?: string; isChecked?: boolean; checked?: boolean }[] }): { data: GroceryListData; checked: Set<string> } {
  const categoryMap = new Map<string, GroceryItemView[]>();
  const checked = new Set<string>();

  for (const item of fullList.items) {
    const cat = item.category || 'Other';
    if (!categoryMap.has(cat)) categoryMap.set(cat, []);
    categoryMap.get(cat)!.push({
      id: item.id,
      name: item.name,
      namePt: item.namePt,
      totalAmount: item.totalAmount,
      unit: item.unit,
    });
    if (item.isChecked || item.checked) checked.add(item.name);
  }

  const categories: Category[] = Array.from(categoryMap.entries()).map(([name, items]) => ({
    name,
    items,
  }));

  const startDate = fullList.startDate?.split('T')[0] || '';
  const endDate = fullList.endDate?.split('T')[0] || '';
  const days = startDate && endDate ? Math.max(1, Math.round((new Date(endDate).getTime() - new Date(startDate).getTime()) / 86400000) + 1) : 3;

  return {
    data: {
      totalItems: fullList.items.length,
      consolidationDays: days,
      dateRange: startDate && endDate ? `${formatShortDate(startDate)} — ${formatShortDate(endDate)}` : '',
      categories,
      listId: fullList.id,
    },
    checked,
  };
}

export function useGroceryList() {
  const [checkedItems, setCheckedItems] = useState<Set<string>>(new Set());
  const [data, setData] = useState<GroceryListData>(emptyData);
  const [generating, setGenerating] = useState(false);
  const [loading, setLoading] = useState(false);
  const activePlanId = useUiStore((s) => s.activeMealPlanId);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  // Auto-load existing grocery list on mount
  useEffect(() => {
    if (!isAuthenticated || !activePlanId) return;

    const loadExisting = async () => {
      setLoading(true);
      try {
        const { data: fullList } = await groceryListsApi.getLatest(activePlanId);
        if (fullList && fullList.items && fullList.items.length > 0) {
          const parsed = parseGroceryList(fullList);
          setData(parsed.data);
          setCheckedItems(parsed.checked);
        }
      } catch {
        // No existing list, that's fine
      } finally {
        setLoading(false);
      }
    };

    loadExisting();
  }, [activePlanId, isAuthenticated]);

  const generate = useCallback(async () => {
    if (!isAuthenticated || !activePlanId || generating) return;
    setGenerating(true);
    try {
      const today = new Date();
      const startDate = today.toISOString().split('T')[0];
      const endDate = new Date(today.getTime() + 2 * 86400000).toISOString().split('T')[0];
      const { data: listData } = await groceryListsApi.generate(activePlanId, startDate, endDate);

      if (listData && listData.id) {
        const { data: fullList } = await groceryListsApi.getById(listData.id);

        if (fullList && fullList.items && fullList.items.length > 0) {
          const parsed = parseGroceryList(fullList);
          setData(parsed.data);
          setCheckedItems(parsed.checked);
        }
      }
    } catch {
      // keep current state on error
    } finally {
      setGenerating(false);
    }
  }, [activePlanId, isAuthenticated, generating]);

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

    if (isAuthenticated && data.listId && itemId) {
      try {
        await groceryListsApi.toggleItem(data.listId, itemId);
      } catch { /* ignore */ }
    }
  }, [isAuthenticated, data.listId]);

  const checkedCount = checkedItems.size;
  const progress = data.totalItems > 0 ? (checkedCount / data.totalItems) * 100 : 0;

  const copyList = useCallback(() => {
    const lines: string[] = [];
    for (const cat of data.categories) {
      for (const item of cat.items) {
        const displayName = item.namePt || item.name;
        const amount = item.totalAmount && item.totalAmount > 0 ? item.totalAmount : null;
        const unitLabel = item.unit ? PT_UNIT_LABELS[item.unit] ?? item.unit : '';
        const checked = checkedItems.has(item.name);
        const prefix = checked ? '- [x]' : '- [ ]';
        if (amount && unitLabel) {
          lines.push(`${prefix} ${displayName} ${amount}${unitLabel}`);
        } else if (amount) {
          lines.push(`${prefix} ${displayName} ${amount}x`);
        } else {
          lines.push(`${prefix} ${displayName}`);
        }
      }
    }
    navigator.clipboard.writeText(lines.join('\n')).catch(() => {});
  }, [data, checkedItems]);

  return {
    data,
    checkedItems,
    toggleItem,
    checkedCount,
    progress,
    generate,
    generating,
    loading,
    copyList,
    hasActivePlan: !!activePlanId,
  };
}

function formatShortDate(dateStr: string): string {
  const d = new Date(dateStr + 'T00:00:00');
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}
