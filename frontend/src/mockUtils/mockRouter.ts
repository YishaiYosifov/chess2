import { useRouter } from "next/navigation";
import { Mock } from "vitest";

export function mockRouter() {
    const router = {
        back: vi.fn(),
        forward: vi.fn(),
        refresh: vi.fn(),
        push: vi.fn(),
        replace: vi.fn(),
        prefetch: vi.fn(),
    };
    const routerMock = useRouter as Mock;
    routerMock.mockImplementation(() => router);

    return router;
}
