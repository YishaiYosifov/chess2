import { GameColor, GameState } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakePlayer } from "./playerFaker";
import { createFakeClock } from "./clockFaker";
import { createFakeMoveSnapshot } from "./moveSnapshotFaker";
import { createFakeMovePath } from "./movePathFaker";

export function createFakeGameState(
    overrides: Partial<GameState> = {},
): GameState {
    return {
        timeControl: {
            baseSeconds: faker.number.int({ min: 10, max: 1000 }),
            incrementSeconds: faker.number.int({ min: 1, max: 10 }),
        },
        isRated: faker.datatype.boolean(),
        whitePlayer: createFakePlayer(GameColor.WHITE),
        blackPlayer: createFakePlayer(GameColor.BLACK),
        clocks: createFakeClock(),
        sideToMove: GameColor.WHITE,
        initialFen: "10/10/10/10/10/10/10/10/10/10",
        moveHistory: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => createFakeMoveSnapshot()),
        moveOptions: {
            legalMoves: Array.from({
                length: faker.number.int({ min: 1, max: 5 }),
            }).map(() => createFakeMovePath()),
            hasForcedMoves: faker.datatype.boolean(),
        },
        ...overrides,
    };
}
