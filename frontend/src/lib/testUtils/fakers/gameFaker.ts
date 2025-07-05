import { faker } from "@faker-js/faker";

import { FinishedGame, GameResult } from "@/types/tempModels";
import { createUser } from "./userFaker";

export function createFinishedGame(
    override?: Partial<FinishedGame>,
): FinishedGame {
    return {
        token: faker.string.uuid(),
        userWhite: createUser(),
        userBlack: createUser(),
        timeControl: 900,
        increment: 1,
        results: faker.helpers.enumValue(GameResult),
        createdAt: Date.now().valueOf(),
        ...override,
    };
}
