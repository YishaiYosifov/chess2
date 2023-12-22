import { GameResult, Variant } from "@/lib/constants";
import { PublicProfile } from "@/lib/types";
import { Game } from "@/lib/types";

export const profileMock: PublicProfile = {
    userId: 1,
    username: "testuser",
    about: "test about",

    country: "ISR",
    pfpLastChanged: "2023-01-01T12:00:00Z",
};

export function creteProfile(
    overrideAttrs: Partial<PublicProfile> = {}
): PublicProfile {
    return { ...profileMock, ...overrideAttrs };
}

export const gameMock: Game = {
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
    createdAt: "2023-01-01T12:00:00Z",
    variant: Variant.Anarchy,
    timeControl: 60,
    increment: 0,
};

export function createGame(overrideAttrs: Partial<Game> = {}): Game {
    return { ...gameMock, ...overrideAttrs };
}
