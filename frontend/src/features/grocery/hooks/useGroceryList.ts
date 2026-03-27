import { useState } from 'react';

interface Category {
  name: string;
  items: string[];
}

interface GroceryListData {
  totalItems: number;
  consolidationDays: number;
  dateRange: string;
  categories: Category[];
}

const mockData: GroceryListData = {
  totalItems: 24,
  consolidationDays: 3,
  dateRange: 'Oct 24 — Oct 26',
  categories: [
    {
      name: 'Produce',
      items: [
        'Organic Baby Spinach',
        'Avocados',
        'Blueberries',
        'Mixed Greens',
        'Tomatoes',
        'Lemons',
        'Bananas',
        'Berries',
      ],
    },
    {
      name: 'Protein',
      items: ['Chicken Breast', 'Salmon Fillet', 'Eggs (dozen)', 'Greek Yogurt'],
    },
    {
      name: 'Dairy',
      items: ['Edam Cheese', 'Cottage Cheese', 'Protein Yogurt', 'Low-fat Milk'],
    },
    {
      name: 'Grains',
      items: ['Bread (whole grain)', 'Rice', 'Oat Flakes', 'Pasta', 'Corn Cakes'],
    },
  ],
};

export function useGroceryList() {
  const [checkedItems, setCheckedItems] = useState<Set<string>>(new Set());

  const toggleItem = (item: string) => {
    setCheckedItems((prev) => {
      const next = new Set(prev);
      if (next.has(item)) {
        next.delete(item);
      } else {
        next.add(item);
      }
      return next;
    });
  };

  const checkedCount = checkedItems.size;
  const progress = mockData.totalItems > 0 ? (checkedCount / mockData.totalItems) * 100 : 0;

  return {
    data: mockData,
    checkedItems,
    toggleItem,
    checkedCount,
    progress,
  };
}
