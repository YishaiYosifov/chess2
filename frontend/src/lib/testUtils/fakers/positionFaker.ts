import { Position } from "@/features/liveGame/lib/types";
import { faker } from "@faker-js/faker";
import { createFakeBoardPieces } from "./chessboardFakers";
import { createFakeSan } from "./sanFaker";

export function createFakePosition(
    overrides: Partial<Position> = {},
): Position {
    return {
        san: createFakeSan(),
        pieces: createFakeBoardPieces(),
        clocks: {
            whiteClock: faker.number.int({ min: 1000, max: 100000 }),
            blackClock: faker.number.int({ min: 1000, max: 100000 }),
        },
        ...overrides,
    };
}
