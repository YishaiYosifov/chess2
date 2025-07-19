import { MoveSideEffectPath } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeMoveSideEffectPath(
    overrides?: Partial<MoveSideEffectPath>,
): MoveSideEffectPath {
    return {
        fromIdx: faker.number.int({ min: 0, max: 99 }),
        toIdx: faker.number.int({ min: 0, max: 99 }),
        ...overrides,
    };
}
