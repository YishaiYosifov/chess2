import { MovePath } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeMoveSideEffectPath } from "./moveSideEffectPathFaker";

export function createFakeMovePath(override?: Partial<MovePath>): MovePath {
    return {
        fromIdx: faker.number.int({ min: 0, max: 99 }),
        toIdx: faker.number.int({ min: 0, max: 99 }),
        moveKey: faker.string.alpha(10),
        capturedIdxs: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => faker.number.int({ min: 0, max: 99 })),
        triggerIdxs: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => faker.number.int({ min: 0, max: 99 })),
        sideEffects: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => createFakeMoveSideEffectPath()),
        ...override,
    };
}
