import { useRouter } from "next/navigation";
import { Mock } from "vitest";

import { GameResult, FinishedGame, PublicUserOut, Variant } from "@/client";

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

export const profileMock: PublicUserOut = {
    userId: 1,
    username: "testuser",
    about: "test about",

    country: "ISR",
    pfpLastChanged: new Date("2023-01-01T12:00:00Z"),
};

export function creteProfile(
    overrideAttrs: Partial<PublicUserOut> = {}
): PublicUserOut {
    return { ...profileMock, ...overrideAttrs };
}

export const gameMock: FinishedGame = {
    token: "test token",
    userWhite: creteProfile({ userId: 1, username: "user-white" }),
    userBlack: creteProfile({ userId: 2, username: "user-black" }),
    results: GameResult.Draw,
    createdAt: new Date("2023-01-01T12:00:00Z"),
    variant: Variant.Anarchy,
    timeControl: 60,
    increment: 0,
};

export function createFinishedGame(
    overrideAttrs: Partial<FinishedGame> = {}
): FinishedGame {
    return { ...gameMock, ...overrideAttrs };
}
