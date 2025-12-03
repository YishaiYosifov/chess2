import { OngoingGame } from "@/features/lobby/lib/types";
import { faker } from "@faker-js/faker";
import { createFakePoolKey } from "./poolKeyFaker";
import { createFakeMinimalProfile } from "./minimalProfileFaker";

export function createFakeOngoingGame(
    overrides?: Partial<OngoingGame>,
): OngoingGame {
    return {
        gameToken: faker.string.alphanumeric(16),
        pool: createFakePoolKey(),
        opponent: createFakeMinimalProfile(),
        ...overrides,
    };
}
