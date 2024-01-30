import { GameResult, FinishedGame, Variant } from "@/client";
import { createProfile } from "./profileMock";

export const gameMock: FinishedGame = {
    token: "test token",
    userWhite: createProfile({ userId: 1, username: "user-white" }),
    userBlack: createProfile({ userId: 2, username: "user-black" }),
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
