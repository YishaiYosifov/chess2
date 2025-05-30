import { HubConnectionBuilder } from "@microsoft/signalr";
import { cookies } from "next/headers";
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

export function mockSignalRConnectionBuilder() {
    const hubConnectionBuilder = {
        withUrl: vi.fn().mockReturnThis(),
        withAutomaticReconnect: vi.fn().mockReturnThis(),
        configureLogging: vi.fn().mockReturnThis(),
        build: vi.fn().mockReturnThis(),
    };
    const hubConnectionBuilderMock = HubConnectionBuilder as Mock;
    hubConnectionBuilderMock.mockReturnValue(hubConnectionBuilder);

    return hubConnectionBuilder;
}

export function mockCookies(...cookieNames: string[]): Mock {
    const cookiesMock = cookies as Mock;
    cookiesMock.mockImplementation(() => ({
        has: (cookieName: string) => cookieNames.includes(cookieName),
    }));

    return cookiesMock;
}
