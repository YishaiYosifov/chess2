import { useRouter } from "next/navigation";
import { Mock } from "vitest";

import { GameResult, GameResults, UserOut, Variant } from "@/client";

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

export const profileMock: UserOut = {
    userId: 1,
    username: "testuser",
    about: "test about",

    country: "ISR",
    pfpLastChanged: new Date("2023-01-01T12:00:00Z"),
};

export function creteProfile(overrideAttrs: Partial<UserOut> = {}): UserOut {
    return { ...profileMock, ...overrideAttrs };
}

export const gameMock: GameResults = {
    token: "test token",
    userWhite: {
        userId: 1,
        username: "user-white",
    },
    userBlack: {
        userId: 2,
        username: "user-black",
    },
    results: GameResult.Draw,
    createdAt: new Date("2023-01-01T12:00:00Z"),
    variant: Variant.Anarchy,
    timeControl: 60,
    increment: 0,
};

export function createGame(
    overrideAttrs: Partial<GameResults> = {}
): GameResults {
    return { ...gameMock, ...overrideAttrs };
}
