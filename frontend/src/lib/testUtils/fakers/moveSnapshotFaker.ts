import { MoveSnapshot } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createMoveSnapshot(
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
        encodedMove: faker.string.alpha(4).toLowerCase(),
        timeLeft: faker.number.int({ min: 1000, max: 600000 }),
        ...overrides,
    };
}
