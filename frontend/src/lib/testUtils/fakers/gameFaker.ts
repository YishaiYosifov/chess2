import { faker } from "@faker-js/faker";

import { FinishedGame } from "@/types/tempModels";
import { createFakeUser } from "./userFaker";
import { GameResult } from "@/lib/apiClient";

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
