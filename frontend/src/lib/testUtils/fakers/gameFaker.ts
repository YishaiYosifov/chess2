import { faker } from "@faker-js/faker";

import { FinishedGame, GameResult } from "@/types/tempModels";
import { createFakeUser } from "./userFaker";

export function createFakeFinishedGame(
    override?: Partial<FinishedGame>,
): FinishedGame {
    return {
        token: faker.string.uuid(),
        userWhite: createFakeUser(),
        userBlack: createFakeUser(),
        timeControl: 900,
        increment: 1,
        results: faker.helpers.enumValue(GameResult),
        createdAt: Date.now().valueOf(),
        ...override,
    };
}
