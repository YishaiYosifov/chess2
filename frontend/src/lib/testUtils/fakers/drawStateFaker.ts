import { DrawState, GameColor } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeDrawState(overrides?: Partial<DrawState>): DrawState {
    return {
        activeRequester: faker.helpers.enumValue(GameColor),
        whiteCooldown: faker.number.int({ min: 10, max: 100 }),
        blackCooldown: faker.number.int({ min: 10, max: 100 }),
        ...overrides,
    };
}
