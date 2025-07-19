import { MoveSnapshot } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeMovePath } from "./movePathFaker";

export function createFakeMoveSnapshot(
    overrides?: Partial<MoveSnapshot>,
): MoveSnapshot {
    return {
        san: faker.helpers.arrayElement([
            "e4",
            "d4",
            "Nf3",
            "Nc6",
            "Bb5",
            "e5",
        ]),
        path: createFakeMovePath(),
        timeLeft: faker.number.int({ min: 1000, max: 600000 }),
        ...overrides,
    };
}
