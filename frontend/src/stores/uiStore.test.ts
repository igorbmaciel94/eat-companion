import { describe, it, expect, beforeEach } from 'vitest';
import { useUiStore } from './uiStore';

describe('uiStore', () => {
  beforeEach(() => {
    // Reset store state between tests
    useUiStore.setState({
      activeMealPlanId: null,
      pendingImportJobId: null,
    });
  });

  it('should initialize with null values', () => {
    const state = useUiStore.getState();
    expect(state.activeMealPlanId).toBeNull();
    expect(state.pendingImportJobId).toBeNull();
  });

  it('should set activeMealPlanId', () => {
    useUiStore.getState().setActiveMealPlanId('plan-123');
    expect(useUiStore.getState().activeMealPlanId).toBe('plan-123');
  });

  it('should clear activeMealPlanId', () => {
    useUiStore.getState().setActiveMealPlanId('plan-123');
    useUiStore.getState().setActiveMealPlanId(null);
    expect(useUiStore.getState().activeMealPlanId).toBeNull();
  });

  it('should set pendingImportJobId', () => {
    useUiStore.getState().setPendingImportJobId('job-456');
    expect(useUiStore.getState().pendingImportJobId).toBe('job-456');
  });

  it('should clear pendingImportJobId', () => {
    useUiStore.getState().setPendingImportJobId('job-456');
    useUiStore.getState().clearPendingImportJobId();
    expect(useUiStore.getState().pendingImportJobId).toBeNull();
  });

  it('should handle activeMealPlanId and pendingImportJobId independently', () => {
    useUiStore.getState().setActiveMealPlanId('plan-123');
    useUiStore.getState().setPendingImportJobId('job-456');

    expect(useUiStore.getState().activeMealPlanId).toBe('plan-123');
    expect(useUiStore.getState().pendingImportJobId).toBe('job-456');

    useUiStore.getState().clearPendingImportJobId();
    expect(useUiStore.getState().activeMealPlanId).toBe('plan-123');
    expect(useUiStore.getState().pendingImportJobId).toBeNull();
  });
});
