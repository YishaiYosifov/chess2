import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import useLiveChessEvents from "../useLiveChessEvents";
import {
    GameClientEvents,
    useGameEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import {
    Clocks,
    DrawState,
    GameColor,
    GameResult,
    GameResultData,
    MovePath,
    MoveSnapshot,
} from "@/lib/apiClient";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { renderHook } from "@testing-library/react";
import { createFakeMoveSnapshot } from "@/lib/testUtils/fakers/moveSnapshotFaker";
import { createFakeClock } from "@/lib/testUtils/fakers/clockFaker";
import { act } from "react";
import { refetchGame } from "../../lib/gameStateProcessor";
import { createFakePosition } from "@/lib/testUtils/fakers/positionFaker";
import { Position } from "../../lib/types";
import {
    createFakeLegalMoveMap,
    createFakePiece,
    createFakePieceMapFromPieces,
    createRandomPoint,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { logicalPoint } from "@/lib/utils/pointUtils";
import { brotliCompressSync } from "zlib";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";

vi.mock("@/features/signalr/hooks/useSignalRHubs");
vi.mock("@/features/liveGame/lib/gameStateProcessor");

describe("useLiveChessEvents", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const useGameEventMock = vi.mocked(useGameEvent);
    const gameEventHandlers: EventHandlers<GameClientEvents> = {};

    const gameOverPopupRef = { current: { open: vi.fn() } };

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEventMock.mockImplementation((_, event, handler) => {
            gameEventHandlers[event] = handler;
        });
    });

    function renderLiveChessEvents() {
        return renderHook(() =>
            useLiveChessEvents(
                liveChessStore,
                chessboardStore,
                gameOverPopupRef,
            ),
        );
    }

    function setupStandardStoresForMove() {
        const piece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        chessboardStore.setState({
            pieces: createFakePieceMapFromPieces(piece),
        });
        liveChessStore.setState({
            viewingMoveNumber: 0,
            positionHistory: [createFakePosition()],
            viewer: { userId: "test id", playerColor: GameColor.WHITE },
        });
        return piece;
    }

    async function triggerMoveMade(
        sideToMove: GameColor,
        clocks: Clocks,
        moveNumber = 1,
    ): Promise<MoveSnapshot> {
        const move = createFakeMoveSnapshot({
            san: "test san",
            path: { fromIdx: 11, toIdx: 12 },
        });
        await act(async () => {
            gameEventHandlers.MoveMadeAsync?.(
                move,
                sideToMove,
                moveNumber,
                clocks,
            );
        });
        return move;
    }

    describe("MoveMadeAsync", () => {
        it("should trigger a refetch when moveNumber is out of sync", async () => {
            liveChessStore.setState({
                positionHistory: [createFakePosition()],
            });

            renderLiveChessEvents();

            const move = createFakeMoveSnapshot();
            const clocks = createFakeClock();

            await act(async () =>
                gameEventHandlers.MoveMadeAsync?.(
                    move,
                    GameColor.WHITE,
                    0,
                    clocks,
                ),
            );

            expect(refetchGame).toHaveBeenCalled();
            expect(liveChessStore.getState().positionHistory.length).toBe(1);
        });

        it.each([true, false])(
            "should only play and store the move if we are not awaiting move ack",
            async (awaitingAck) => {
                setupStandardStoresForMove();
                renderLiveChessEvents();

                const clocks = createFakeClock();
                const piecesBefore = chessboardStore.getState().pieces;
                const positionHistoryBefore =
                    liveChessStore.getState().positionHistory;
                if (awaitingAck) liveChessStore.getState().markPendingMoveAck();

                const move = await triggerMoveMade(GameColor.WHITE, clocks);

                expect(liveChessStore.getState().viewingMoveNumber).toBe(1);
                expect(liveChessStore.getState().positionHistory.length).toBe(
                    positionHistoryBefore.length + 1,
                );

                const piecesAfter = chessboardStore.getState().pieces;
                if (!awaitingAck) {
                    expect(piecesAfter).not.toEqual(piecesBefore);
                } else {
                    expect(piecesAfter).toEqual(piecesBefore);
                }

                expect(
                    liveChessStore.getState().positionHistory[1],
                ).toEqual<Position>({
                    san: move.san,
                    pieces: piecesAfter,
                    clocks: {
                        whiteClock: clocks.whiteClock,
                        blackClock: clocks.blackClock,
                    },
                });
            },
        );

        it.each([
            [GameColor.WHITE, GameColor.BLACK],
            [GameColor.WHITE, GameColor.WHITE],
        ])(
            "should only disable movement if the side to move !== us",
            async (ourColor, newSideToMove) => {
                liveChessStore.setState({
                    viewer: { userId: "test id", playerColor: ourColor },
                });
                chessboardStore.setState({
                    moveOptions: createMoveOptions({
                        legalMoves: createFakeLegalMoveMap(),
                    }),
                });

                setupStandardStoresForMove();
                renderLiveChessEvents();

                const move = createFakeMoveSnapshot();
                const clocks = createFakeClock();
                await act(async () =>
                    gameEventHandlers.MoveMadeAsync?.(
                        move,
                        newSideToMove,
                        1,
                        clocks,
                    ),
                );

                const moveOptions = chessboardStore.getState().moveOptions;
                if (ourColor !== newSideToMove) {
                    expect(moveOptions.legalMoves.size).toBe(0);
                } else {
                    expect(moveOptions.legalMoves.size).not.toBe(0);
                }
            },
        );

        it("should jump forward if we are viewing past moves", async () => {
            liveChessStore.setState({
                viewingMoveNumber: 0,
                positionHistory: [createFakePosition(), createFakePosition()],
                viewer: { userId: "test id", playerColor: GameColor.WHITE },
            });

            renderLiveChessEvents();

            const move = createFakeMoveSnapshot({
                san: "test san",
                path: { fromIdx: 11, toIdx: 12 },
            });
            const clocks = createFakeClock();

            await act(async () =>
                gameEventHandlers.MoveMadeAsync?.(
                    move,
                    GameColor.WHITE,
                    2,
                    clocks,
                ),
            );

            expect(liveChessStore.getState().viewingMoveNumber).toBe(2);
        });
    });

    describe("LegalMovesChangedAsync", () => {
        function encodeMoves(moves: MovePath[]): string {
            const json = JSON.stringify(moves);
            const compressed = brotliCompressSync(Buffer.from(json));
            return compressed.toString("base64");
        }

        it("should decode legal moves and update both stores", async () => {
            liveChessStore.setState({
                latestMoveOptions: {
                    legalMoves: new Map(),
                    hasForcedMoves: false,
                },
            });

            chessboardStore.setState({
                moveOptions: { legalMoves: new Map(), hasForcedMoves: false },
            });

            renderLiveChessEvents();
            const fakeMoves: MovePath[] = [
                {
                    fromIdx: 0,
                    toIdx: 1,
                    triggerIdxs: [2],
                    capturedIdxs: [3],
                    sideEffects: [{ fromIdx: 4, toIdx: 5 }],
                    promotesTo: null,
                },
                {
                    fromIdx: 10,
                    toIdx: 11,
                },
            ];
            const encodedMoves = encodeMoves(fakeMoves);
            const hasForcedMoves = true;

            await act(async () =>
                gameEventHandlers.LegalMovesChangedAsync?.(
                    encodedMoves,
                    hasForcedMoves,
                ),
            );

            const liveState = liveChessStore.getState();
            expect(liveState.latestMoveOptions.hasForcedMoves).toBe(
                hasForcedMoves,
            );
            expect(liveState.latestMoveOptions.legalMoves.size).toBeGreaterThan(
                0,
            );

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.moveOptions.hasForcedMoves).toBe(
                hasForcedMoves,
            );
            expect(chessboardState.moveOptions.legalMoves.size).toBeGreaterThan(
                0,
            );
        });
    });

    describe("DrawStateChangeAsync", () => {
        it("should update the drawState in liveChessStore", () => {
            const initialDrawState = {
                activeRequester: null,
                whiteCooldown: 0,
                blackCooldown: 0,
            };

            liveChessStore.setState({
                drawState: initialDrawState,
            });

            renderLiveChessEvents();

            const newDrawState: DrawState = {
                activeRequester: GameColor.WHITE,
                whiteCooldown: 5,
                blackCooldown: 3,
            };

            act(() => {
                gameEventHandlers.DrawStateChangeAsync?.(newDrawState);
            });

            expect(liveChessStore.getState().drawState).toEqual(newDrawState);
        });
    });

    describe("GameEndedAsync", () => {
        it("should update liveChessStore, disable chessboard movement, and open the game over popup", async () => {
            liveChessStore.setState({
                resultData: null,
                latestMoveOptions: {
                    legalMoves: createFakeLegalMoveMap(),
                    hasForcedMoves: false,
                },
            });
            chessboardStore.setState({
                moveOptions: {
                    legalMoves: createFakeLegalMoveMap(),
                    hasForcedMoves: false,
                },
                highlightedLegalMoves: [
                    createRandomPoint(),
                    createRandomPoint(),
                ],
                selectedPieceId: "123",
            });

            const openMock = vi.fn();
            gameOverPopupRef.current = { open: openMock };

            renderLiveChessEvents();

            const gameResult: GameResultData = {
                whiteRatingChange: 10,
                blackRatingChange: -10,
                result: GameResult.WHITE_WIN,
                resultDescription: "test",
            };

            await act(async () => {
                gameEventHandlers.GameEndedAsync?.(gameResult);
            });

            const liveState = liveChessStore.getState();
            expect(liveState.resultData).toEqual(gameResult);

            const chessboardState = chessboardStore.getState();
            expect(chessboardState.highlightedLegalMoves).toHaveLength(0);
            expect(chessboardState.selectedPieceId).toBeNull();
            expect(chessboardState.moveOptions.legalMoves.size).toBe(0);

            expect(openMock).toHaveBeenCalled();
        });
    });
});
