import { GameColor } from "@/lib/apiClient";
import { createStoreProps } from "../gameStateProcessor";
import { createFakeGameState } from "@/lib/testUtils/fakers/gameStateFaker";
import { ChessboardProps } from "@/features/chessboard/stores/chessboardStore";
import { LiveChessStoreProps } from "../../stores/liveChessStore";
import { LogicalPoint } from "@/features/point/types";
import { Position } from "../types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import constants from "@/lib/constants";
import { simulateMove } from "@/features/chessboard/lib/simulateMove";
import { logicalPoint } from "@/features/point/pointUtils";
import { decodePath, decodePathIntoMap } from "../moveDecoder";
import { LiveChessViewer } from "../../stores/gamePlaySlice";

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
                        moveKey: "0",
                    },
                    san: "f5",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 85,
                        toIdx: 55,
                        moveKey: "1",
                    },
                    san: "f6",
                    timeLeft: 100,
                },
                {
                    path: {
                        fromIdx: 8,
                        toIdx: 27,
                        moveKey: "2",
                    },
                    san: "Hh3",
                    timeLeft: 50,
                },
                {
                    path: {
                        fromIdx: 91,
                        toIdx: 72,
                        moveKey: "3",
                    },
                    san: "Hc8",
                    timeLeft: 50,
                },
            ],
            moveOptions: {
                legalMoves: [
                    { fromIdx: 0, toIdx: 1, moveKey: "0" },
                    { fromIdx: 2, toIdx: 3, moveKey: "1" },
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
                moveKey: "",
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
                decoded: decodePath(gameState.moveHistory[0].path, 10),
                clocks: { whiteClock: 100, blackClock: baseMs },
                san: "f5",
            },
            {
                from: logicalPoint({ x: 5, y: 8 }),
                to: logicalPoint({ x: 5, y: 5 }),
                decoded: decodePath(gameState.moveHistory[1].path, 10),
                clocks: { whiteClock: 100, blackClock: 100 },
                san: "f6",
            },
            {
                from: logicalPoint({ x: 8, y: 0 }),
                to: logicalPoint({ x: 7, y: 2 }),
                decoded: decodePath(gameState.moveHistory[2].path, 10),
                clocks: { whiteClock: 50, blackClock: 100 },
                san: "Hh3",
            },
            {
                from: logicalPoint({ x: 1, y: 9 }),
                to: logicalPoint({ x: 2, y: 7 }),
                decoded: decodePath(gameState.moveHistory[3].path, 10),
                clocks: { whiteClock: 50, blackClock: 50 },
                san: "Hc8",
            },
        ];

        for (const move of moves) {
            applyMove(move.from, move.to);
            positionHistory.push({
                pieces,
                move: move.decoded,
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
                pieceMap: pieces,
                moveOptions: latestMoveOptions,
                boardDimensions: {
                    width: constants.BOARD_WIDTH,
                    height: constants.BOARD_HEIGHT,
                },
                viewingFrom: GameColor.BLACK,
                canDrag: true,
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
