import { useRouter } from "next/navigation";

export function mockRouter() {
    const router = {
        back: vi.fn(),
        forward: vi.fn(),
        refresh: vi.fn(),
        push: vi.fn(),
        replace: vi.fn(),
        prefetch: vi.fn(),
    };
    const routerMock = vi.mocked(useRouter);
    routerMock.mockImplementation(() => router);

    return router;
}
