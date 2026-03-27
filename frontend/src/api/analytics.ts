import client from './client';

export const analyticsApi = {
  getAdherence: (weeks: number = 4) =>
    client.get('/analytics/adherence', { params: { weeks } }),
  getWeightTrend: (months: number = 3) =>
    client.get('/analytics/weight-trend', { params: { months } }),
};
