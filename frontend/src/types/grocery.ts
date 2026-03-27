export interface GroceryItem {
  id: string;
  name: string;
  quantity: number;
  unit: string;
  category: string;
  checked: boolean;
  mealPlanId?: string;
}

export interface GroceryList {
  id: string;
  mealPlanId: string;
  name: string;
  items: GroceryItem[];
  createdAt: string;
  updatedAt: string;
}
