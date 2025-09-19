import { GameColor } from "@/lib/apiClient";
import { createStoreProps } from "../gameStateProcessor";
import { createFakeGameState } from "@/lib/testUtils/fakers/gameStateFaker";
import { ChessboardProps } from "@/features/chessboard/stores/chessboardStore";
import {
    LiveChessStoreProps,
    LiveChessViewer,
} from "../../stores/liveChessStore";
import { LogicalPoint } from "@/features/point/types";
import { Position } from "../types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import constants from "@/lib/constants";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import { logicalPoint } from "@/lib/utils/pointUtils";
import { decodePathIntoMap } from "../moveDecoder";

describe("createStoreProps", () => {
    it("should return the complete and correct store props object", () => {
        const gameState = createFakeGameState({
            initialFen: constants.INITIAL_FEN,
            // f5 f6 Nh3 Nc8
            moveHistory: [
                {
                    path: {
                        fromIdx: 15,
                        toIdx: 45,
                    },
                    san: "f5",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 85,
                        toIdx: 55,
                    },
                    san: "f6",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 8,
                        toIdx: 27,
                    },
                    san: "Hh3",
                    timeLeft: 50,
                },
                {
                    path: {
                        fromIdx: 91,
                        toIdx: 72,
                    },
                    san: "Hc8",
                    timeLeft: 50,
                },
            ],
            moveOptions: {
                legalMoves: [
                    { fromIdx: 0, toIdx: 1 },
                    { fromIdx: 2, toIdx: 3 },
                ],
                hasForcedMoves: true,
            },
            drawState: {
                activeRequester: GameColor.WHITE,
                whiteCooldown: 6,
                blackCooldown: 9,
            },
        });

        const result = createStoreProps(
            "game-token",
            gameState.blackPlayer.userId,
            gameState,
        );

        const baseMs = gameState.pool.timeControl.baseSeconds * 1000;
        let pieces = new Map(constants.DEFAULT_CHESS_BOARD);

        function applyMove(from: LogicalPoint, to: LogicalPoint) {
            const { newPieces } = simulateMove(pieces, {
                from,
                to,
                captures: [],
                triggers: [],
                intermediates: [],
                sideEffects: [],
                promotesTo: null,
            });
            pieces = newPieces;
        }

        // starting with initial position
        const positionHistory: Position[] = [
            {
                pieces: new Map(pieces),
                clocks: {
                    whiteClock: baseMs,
                    blackClock: baseMs,
                },
            },
        ];

        // moves and clocks from the test setup
        const moves = [
            {
                from: logicalPoint({ x: 5, y: 1 }),
                to: logicalPoint({ x: 5, y: 4 }),
                clocks: { whiteClock: 100, blackClock: baseMs },
                san: "f5",
            },
            {
                from: logicalPoint({ x: 5, y: 8 }),
                to: logicalPoint({ x: 5, y: 5 }),
                clocks: { whiteClock: 100, blackClock: 100 },
                san: "f6",
            },
            {
                from: logicalPoint({ x: 8, y: 0 }),
                to: logicalPoint({ x: 7, y: 2 }),
                clocks: { whiteClock: 50, blackClock: 100 },
                san: "Hh3",
            },
            {
                from: logicalPoint({ x: 1, y: 9 }),
                to: logicalPoint({ x: 2, y: 7 }),
                clocks: { whiteClock: 50, blackClock: 50 },
                san: "Hc8",
            },
        ];

        for (const move of moves) {
            applyMove(move.from, move.to);
            positionHistory.push({
                pieces,
                clocks: move.clocks,
                san: move.san,
            });
        }

        const legalMoves = decodePathIntoMap(
            gameState.moveOptions.legalMoves,
            constants.BOARD_WIDTH,
        );
        const latestMoveOptions: ProcessedMoveOptions = {
            legalMoves,
            hasForcedMoves: true,
        };

        expect(result).toEqual<{
            live: LiveChessStoreProps;
            board: ChessboardProps;
        }>({
            live: {
                gameToken: "game-token",
                whitePlayer: gameState.whitePlayer,
                blackPlayer: gameState.blackPlayer,
                sideToMove: gameState.sideToMove,
                positionHistory,

                pool: gameState.pool,
                viewer: {
                    userId: gameState.blackPlayer.userId,
                    playerColor: GameColor.BLACK,
                },

                viewingMoveNumber: 4,
                latestMoveOptions,

                drawState: gameState.drawState,
                clocks: gameState.clocks,
                resultData: null,
            },
            board: {
                pieces,
                moveOptions: latestMoveOptions,
                boardDimensions: {
                    width: constants.BOARD_WIDTH,
                    height: constants.BOARD_HEIGHT,
                },
                viewingFrom: GameColor.BLACK,
            },
        });
    });

    it("should return the right viewer for spectator", () => {
        const gameState = createFakeGameState();
        const userId = "random user id";

        const result = createStoreProps("game-token", userId, gameState);

        expect(result.live.viewer).toEqual<LiveChessViewer>({
            userId,
            playerColor: null,
        });
    });
});
