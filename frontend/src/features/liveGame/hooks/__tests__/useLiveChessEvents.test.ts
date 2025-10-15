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
import { logicalPoint } from "@/features/point/pointUtils";
import { brotliCompressSync } from "zlib";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";
import { decodePath } from "../../lib/moveDecoder";
import { GameClientEvents, useGameEvent } from "../useGameHub";

vi.mock("@/features/liveGame/hooks/useGameHub");
vi.mock("@/features/liveGame/lib/gameStateProcessor");

describe("useLiveChessEvents", () => {
    let liveChessStore: StoreApi<LiveChessStore>;
    let chessboardStore: StoreApi<ChessboardStore>;

    const useGameEventMock = vi.mocked(useGameEvent);
    const gameEventHandlers: EventHandlers<GameClientEvents> = {};

    beforeEach(() => {
        liveChessStore = createLiveChessStore(createFakeLiveChessStoreProps());
        chessboardStore = createChessboardStore();

        useGameEventMock.mockImplementation((_, event, handler) => {
            gameEventHandlers[event] = handler;
        });
    });

    function renderLiveChessEvents() {
        return renderHook(() =>
            useLiveChessEvents(liveChessStore, chessboardStore),
        );
    }

    function setupStandardStoresForMove() {
        const piece = createFakePiece({
            position: logicalPoint({ x: 1, y: 1 }),
        });
        chessboardStore.setState({
            pieceMap: createFakePieceMapFromPieces(piece),
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
            path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
        });
        await act(async () => {
            await gameEventHandlers.MoveMadeAsync?.(
                move,
                sideToMove,
                moveNumber,
                clocks,
            );
        });
        return move;
    }

    describe("SyncRevisionAsync", () => {
        it("should refetch the game if revision is out of sync", async () => {
            const newRevision = 2;

            liveChessStore.setState({
                revision: 1,
            });

            renderLiveChessEvents();

            await act(async () => {
                await gameEventHandlers.SyncRevisionAsync?.(newRevision);
            });

            expect(refetchGame).toHaveBeenCalledWith(
                liveChessStore,
                chessboardStore,
            );
        });

        it("should not refetch if revision matches", async () => {
            const newRevision = 1;

            liveChessStore.setState({
                revision: newRevision,
            });

            renderLiveChessEvents();

            await act(async () => {
                await gameEventHandlers.SyncRevisionAsync?.(newRevision);
            });

            expect(refetchGame).not.toHaveBeenCalled();
        });
    });

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
                const piecesBefore = chessboardStore.getState().pieceMap;
                const positionHistoryBefore =
                    liveChessStore.getState().positionHistory;
                if (awaitingAck) liveChessStore.getState().markPendingMoveAck();

                const move = await triggerMoveMade(GameColor.WHITE, clocks);

                expect(liveChessStore.getState().viewingMoveNumber).toBe(1);
                expect(liveChessStore.getState().positionHistory.length).toBe(
                    positionHistoryBefore.length + 1,
                );

                const piecesAfter = chessboardStore.getState().pieceMap;
                if (!awaitingAck) {
                    expect(piecesAfter).not.toEqual(piecesBefore);
                } else {
                    expect(piecesAfter).toEqual(piecesBefore);
                }

                expect(
                    liveChessStore.getState().positionHistory[1],
                ).toEqual<Position>({
                    san: move.san,
                    move: decodePath(move.path, 10),
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
                    latestMoveOptions: createMoveOptions({
                        legalMoves: createFakeLegalMoveMap(),
                    }),
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
                await act(
                    async () =>
                        await gameEventHandlers.MoveMadeAsync?.(
                            move,
                            newSideToMove,
                            1,
                            clocks,
                        ),
                );

                const moveOptions = chessboardStore.getState().moveOptions;
                const latestPositionMoveOptions =
                    liveChessStore.getState().latestMoveOptions;
                if (ourColor !== newSideToMove) {
                    expect(moveOptions.legalMoves.size).toBe(0);
                    expect(latestPositionMoveOptions.legalMoves.size).toBe(0);
                } else {
                    expect(moveOptions.legalMoves.size).not.toBe(0);
                    expect(latestPositionMoveOptions.legalMoves.size).not.toBe(
                        0,
                    );
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
                path: { fromIdx: 11, toIdx: 12, moveKey: "0" },
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
                    moveKey: "1",
                    triggerIdxs: [2],
                    capturedIdxs: [3],
                    sideEffects: [{ fromIdx: 4, toIdx: 5 }],
                    promotesTo: null,
                },
                {
                    fromIdx: 10,
                    toIdx: 11,
                    moveKey: "2",
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
        });
    });
});
