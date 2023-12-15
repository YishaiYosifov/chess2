import { useRouter } from "next/navigation";

export function mockRouter() {
    const router = {
        back: jest.fn(),
        forward: jest.fn(),
        refresh: jest.fn(),
        push: jest.fn(),
        replace: jest.fn(),
        prefetch: jest.fn(),
    };
    const routerMock = useRouter as jest.Mock;
    routerMock.mockImplementation(() => router);

    return router;
}
