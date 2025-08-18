import { LiveChessStoreProps } from "@/features/liveGame/stores/liveChessStore";
import { GameColor, GamePlayer, PoolType } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakePlayer } from "./playerFaker";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { createFakePosition } from "./positionFaker";
import { createFakeClock } from "./clockFaker";
import { createFakeLegalMoveMap } from "./chessboardFakers";

export function createFakeLiveChessStoreProps(
    override: Partial<LiveChessStoreProps> & {
        viewerColor?: GameColor | null;
    } = {},
): LiveChessStoreProps {
    const positionHistory = override?.positionHistory ?? [
        createFakePosition(),
        createFakePosition(),
    ];

    const whitePlayer = createFakePlayer(GameColor.WHITE);
    const blackPlayer = createFakePlayer(GameColor.BLACK);

    const viewerColor =
        override.viewerColor === undefined
            ? GameColor.WHITE
            : override.viewerColor;
    const viewer = createFakeViewer(whitePlayer, blackPlayer, viewerColor);

    return {
        gameToken: faker.string.alpha(16),
        positionHistory,
        viewingMoveNumber: positionHistory.length - 1,
        latestMoveOptions: createMoveOptions({
            legalMoves: createFakeLegalMoveMap(),
        }),

        sideToMove: faker.helpers.enumValue(GameColor),
        viewer,
        whitePlayer: whitePlayer,
        blackPlayer: blackPlayer,

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

function createFakeViewer(
    whitePlayer: GamePlayer,
    blackPlayer: GamePlayer,
    viewerColor: GameColor | null,
) {
    switch (viewerColor) {
        case GameColor.WHITE:
            return { playerColor: GameColor.WHITE, userId: whitePlayer.userId };
        case GameColor.BLACK:
            return { playerColor: GameColor.BLACK, userId: blackPlayer.userId };
        default:
            return { playerColor: null, userId: faker.string.uuid() };
    }
}
