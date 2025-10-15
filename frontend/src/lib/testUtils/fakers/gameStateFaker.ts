import { GameColor, GameState } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakePlayer } from "./playerFaker";
import { createFakeClock } from "./clockFaker";
import { createFakeMoveSnapshot } from "./moveSnapshotFaker";
import { createFakeMovePath } from "./movePathFaker";
import constants from "@/lib/constants";
import { createFakePoolKey } from "./poolKeyFaker";

export function createFakeGameState(
    overrides: Partial<GameState> = {},
): GameState {
    return {
        pool: createFakePoolKey(),
        revision: faker.number.int({ min: 5, max: 100 }),
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
