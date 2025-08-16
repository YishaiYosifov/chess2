import { GameColor, GameState, PoolType } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakePlayer } from "./playerFaker";
import { createFakeClock } from "./clockFaker";
import { createFakeMoveSnapshot } from "./moveSnapshotFaker";
import { createFakeMovePath } from "./movePathFaker";
import constants from "@/lib/constants";

export function createFakeGameState(
    overrides: Partial<GameState> = {},
): GameState {
    return {
        pool: {
            poolType: faker.helpers.enumValue(PoolType),
            timeControl: {
                baseSeconds: faker.number.int({ min: 10, max: 1000 }),
                incrementSeconds: faker.number.int({ min: 1, max: 10 }),
            },
        },
        whitePlayer: createFakePlayer(GameColor.WHITE),
        blackPlayer: createFakePlayer(GameColor.BLACK),
        sideToMove: GameColor.WHITE,

        initialFen: constants.INITIAL_FEN,
        moveHistory: Array.from({
            length: faker.number.int({ min: 1, max: 5 }),
        }).map(() => createFakeMoveSnapshot()),

        clocks: createFakeClock(),
        drawState: {
            activeRequester: null,
            whiteCooldown: 0,
            blackCooldown: 0,
        },
        moveOptions: {
            legalMoves: Array.from({
                length: faker.number.int({ min: 1, max: 5 }),
            }).map(() => createFakeMovePath()),
            hasForcedMoves: faker.datatype.boolean(),
        },
        ...overrides,
    };
}
