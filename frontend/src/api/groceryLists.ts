import client from './client';

export const groceryListsApi = {
  generate: (mealPlanId: string, startDate: string, endDate: string) =>
    client.post('/grocery-lists/generate', { mealPlanId, startDate, endDate }),
  getById: (id: string) => client.get(`/grocery-lists/${id}`),
  toggleItem: (listId: string, itemId: string) =>
    client.put(`/grocery-lists/${listId}/items/${itemId}/toggle`),
};
