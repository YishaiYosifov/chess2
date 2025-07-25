import { Position } from "@/types/tempModels";
import { faker } from "@faker-js/faker";
import { createFakePieceMap } from "./chessboardFakers";
import { createFakeSan } from "./sanFaker";

export function createFakePosition(
    overrides: Partial<Position> = {},
): Position {
    return {
        san: createFakeSan(),
        pieces: createFakePieceMap(),
        clocks: {
            whiteClock: faker.number.int({ min: 1000, max: 100000 }),
            blackClock: faker.number.int({ min: 1000, max: 100000 }),
        },
        ...overrides,
    };
}
