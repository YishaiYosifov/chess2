import { Position } from "@/types/tempModels";
import { faker } from "@faker-js/faker";
import { createFakePieceMap } from "./chessboardFakers";

export function createFakePosition(
    overrides: Partial<Position> = {},
): Position {
    return {
        san: faker.helpers.arrayElement([
            "e4",
            "d4",
            "Nf3",
            "Nc6",
            "Bb5",
            "e5",
        ]),
        pieces: createFakePieceMap(),
        clocks: {
            whiteClock: faker.number.int({ min: 1000, max: 100000 }),
            blackClock: faker.number.int({ min: 1000, max: 100000 }),
        },
        ...overrides,
    };
}
