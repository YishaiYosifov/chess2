import { faker } from "@faker-js/faker";

import { GameResult, GameSummary } from "@/lib/apiClient";
import { createFakePlayerSummary } from "./playerSummaryFaker";

export function createFakeGameSummary(
    override?: Partial<GameSummary>,
): GameSummary {
    return {
        gameToken: faker.string.uuid(),
        whitePlayer: createFakePlayerSummary(),
        blackPlayer: createFakePlayerSummary(),
        result: faker.helpers.enumValue(GameResult),
        createdAt: Date.now().toLocaleString(),
        ...override,
    };
}
