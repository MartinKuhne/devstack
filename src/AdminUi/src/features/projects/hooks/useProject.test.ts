import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook } from '@testing-library/react';

vi.mock('@/generated/graphql', () => ({
    useGetProjectQuery: vi.fn(),
    useGetProjectsQuery: vi.fn(),
}));

const getMockedQueries = async () => {
    const m = await import('@/generated/graphql');
    return {
        useGetProjectQuery: m.useGetProjectQuery as ReturnType<typeof vi.fn>,
        useGetProjectsQuery: m.useGetProjectsQuery as ReturnType<typeof vi.fn>,
    };
};

describe('useProject', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('returns the project from data', async () => {
        const hooks = await getMockedQueries();
        const mockProject = { id: 'proj-1', name: 'Test Project' } as any;
        hooks.useGetProjectQuery.mockReturnValue({
            data: { project: mockProject },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProject } = await import('./useProject');
        const { result } = renderHook(() => useProject('proj-1'));

        expect(result.current.project).toBe(mockProject);
    });

    it('returns null when no data', async () => {
        const hooks = await getMockedQueries();
        hooks.useGetProjectQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProject } = await import('./useProject');
        const { result } = renderHook(() => useProject('proj-1'));

        expect(result.current.project).toBeNull();
    });

    it('skips query when id is falsy', async () => {
        const hooks = await getMockedQueries();
        hooks.useGetProjectQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProject } = await import('./useProject');
        renderHook(() => useProject(''));

        expect(hooks.useGetProjectQuery).toHaveBeenCalledWith(
            expect.objectContaining({ skip: true }),
        );
    });

    it('returns loading state', async () => {
        const hooks = await getMockedQueries();
        hooks.useGetProjectQuery.mockReturnValue({
            data: undefined,
            loading: true,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProject } = await import('./useProject');
        const { result } = renderHook(() => useProject('proj-1'));

        expect(result.current.loading).toBe(true);
    });

    it('returns error when present', async () => {
        const hooks = await getMockedQueries();
        const mockError = { message: 'Not found' };
        hooks.useGetProjectQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: mockError,
            refetch: vi.fn(),
        });

        const { useProject } = await import('./useProject');
        const { result } = renderHook(() => useProject('proj-1'));

        expect(result.current.error).toBe(mockError);
    });

    it('returns refetch function', async () => {
        const hooks = await getMockedQueries();
        const mockRefetch = vi.fn();
        hooks.useGetProjectQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: undefined,
            refetch: mockRefetch,
        });

        const { useProject } = await import('./useProject');
        const { result } = renderHook(() => useProject('proj-1'));

        expect(result.current.refetch).toBe(mockRefetch);
    });
});

describe('useProjects', () => {
    beforeEach(async () => {
        vi.clearAllMocks();
    });

    it('returns projects from data', async () => {
        const hooks = await getMockedQueries();
        const mockProjects = [
            { id: 'proj-1', name: 'Project 1' },
            { id: 'proj-2', name: 'Project 2' },
        ];
        hooks.useGetProjectsQuery.mockReturnValue({
            data: { projects: { nodes: mockProjects } },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProjects } = await import('./useProjects');
        const { result } = renderHook(() => useProjects());

        expect(result.current.projects).toHaveLength(2);
        expect(result.current.projects[0]!.name).toBe('Project 1');
    });

    it('returns empty array when no projects', async () => {
        const hooks = await getMockedQueries();
        hooks.useGetProjectsQuery.mockReturnValue({
            data: { projects: { nodes: [] } },
            loading: false,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProjects } = await import('./useProjects');
        const { result } = renderHook(() => useProjects());

        expect(result.current.projects).toHaveLength(0);
    });

    it('returns loading state', async () => {
        const hooks = await getMockedQueries();
        hooks.useGetProjectsQuery.mockReturnValue({
            data: undefined,
            loading: true,
            error: undefined,
            refetch: vi.fn(),
        });

        const { useProjects } = await import('./useProjects');
        const { result } = renderHook(() => useProjects());

        expect(result.current.loading).toBe(true);
    });

    it('returns error when present', async () => {
        const hooks = await getMockedQueries();
        const mockError = { message: 'Network error' };
        hooks.useGetProjectsQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: mockError,
            refetch: vi.fn(),
        });

        const { useProjects } = await import('./useProjects');
        const { result } = renderHook(() => useProjects());

        expect(result.current.error).toBe(mockError);
    });

    it('returns refetch function', async () => {
        const hooks = await getMockedQueries();
        const mockRefetch = vi.fn();
        hooks.useGetProjectsQuery.mockReturnValue({
            data: undefined,
            loading: false,
            error: undefined,
            refetch: mockRefetch,
        });

        const { useProjects } = await import('./useProjects');
        const { result } = renderHook(() => useProjects());

        expect(result.current.refetch).toBe(mockRefetch);
    });
});
