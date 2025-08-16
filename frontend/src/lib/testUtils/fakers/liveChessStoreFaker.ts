import { LiveChessStoreProps } from "@/features/liveGame/stores/liveChessStore";
import { GameColor, PoolType } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakePlayer } from "./playerFaker";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { createFakePosition } from "./positionFaker";
import { createFakeClock } from "./clockFaker";
import { createFakeLegalMoveMap } from "./chessboardFakers";

export function createFakeLiveChessStoreProps(
    override?: Partial<LiveChessStoreProps>,
): LiveChessStoreProps {
    const positionHistory = override?.positionHistory ?? [
        createFakePosition(),
        createFakePosition(),
    ];

    return {
        gameToken: faker.string.alpha(16),
        positionHistory,
        viewingMoveNumber: positionHistory.length - 1,
        latestMoveOptions: createMoveOptions({
            legalMoves: createFakeLegalMoveMap(),
        }),

        sideToMove: faker.helpers.enumValue(GameColor),
        playerColor: faker.helpers.enumValue(GameColor),
        whitePlayer: createFakePlayer(GameColor.WHITE),
        blackPlayer: createFakePlayer(GameColor.BLACK),

        userId: faker.string.uuid(),
        pool: {
            poolType: faker.helpers.enumValue(PoolType),
            timeControl: {
                baseSeconds: faker.number.int({ min: 60, max: 1200 }),
                incrementSeconds: faker.number.int({ min: 3, max: 30 }),
            },
        },

        clocks: createFakeClock(),
        drawState: {
            activeRequester: null,
            whiteCooldown: 0,
            blackCooldown: 0,
        },
        resultData: null,

        ...override,
    };
}
