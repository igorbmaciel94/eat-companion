import client from './client';
import type { GroceryList } from '../types';

export const groceryListsApi = {
  getByMealPlan: (mealPlanId: string) =>
    client.get<GroceryList>(`/meal-plans/${mealPlanId}/grocery-list`),
  toggleItem: (listId: string, itemId: string, checked: boolean) =>
    client.put(`/grocery-lists/${listId}/items/${itemId}`, { checked }),
};
