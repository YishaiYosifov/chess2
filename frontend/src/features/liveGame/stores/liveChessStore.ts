import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import {
    Clocks,
    DrawState,
    GameColor,
    GamePlayer,
    GameResultData,
    PoolKey,
} from "@/lib/apiClient";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";
import { BoardState } from "../lib/types";
import { Position } from "../lib/types";
import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";

export interface LiveChessViewer {
    userId: string;
    playerColor: GameColor | null;
}

export interface LiveChessStoreProps {
    gameToken: string;
    positionHistory: Position[];
    viewingMoveNumber: number;
    latestMoveOptions: ProcessedMoveOptions;

    sideToMove: GameColor;
    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;

    pool: PoolKey;
    viewer: LiveChessViewer;

    clocks: Clocks;
    drawState: DrawState;
    resultData: GameResultData | null;
}

export interface LiveChessStore extends LiveChessStoreProps {
    isPendingMoveAck: boolean;

    receiveMove(
        position: Position,
        clocks: Clocks,
        sideToMove: GameColor,
    ): void;
    resetLegalMovesForOpponentTurn(): void;
    receiveLegalMoves(moveOptions: ProcessedMoveOptions): void;
    drawStateChange(drawState: DrawState): void;
    markPendingMoveAck(): void;

    teleportToMove(number: number): BoardState | undefined;
    shiftMoveViewBy(amount: number): BoardState | undefined;
    teleportToLastMove(): BoardState;

    endGame(resultData: GameResultData): void;
    resetState(initState: LiveChessStoreProps): void;
}

enableMapSet();
export default function createLiveChessStore(initState: LiveChessStoreProps) {
    return createWithEqualityFn<LiveChessStore>()(
        immer((set, get, store) => ({
            ...initState,
            isPendingMoveAck: false,

            receiveMove(position, clocks, sideToMove) {
                const { viewingMoveNumber, positionHistory } = get();

                set((state) => {
                    if (viewingMoveNumber === positionHistory.length - 1)
                        state.viewingMoveNumber++;

                    state.drawState.whiteCooldown = Math.max(
                        0,
                        state.drawState.whiteCooldown - 1,
                    );
                    state.drawState.blackCooldown = Math.max(
                        0,
                        state.drawState.blackCooldown - 1,
                    );

                    state.positionHistory.push(position);
                    state.clocks = clocks;
                    state.sideToMove = sideToMove;
                    state.isPendingMoveAck = false;
                });
            },
            resetLegalMovesForOpponentTurn() {
                set((state) => {
                    state.latestMoveOptions = createMoveOptions();
                });
            },
            receiveLegalMoves(moveOptions) {
                set((state) => {
                    state.latestMoveOptions = moveOptions;
                });
            },
            drawStateChange(drawState) {
                set((state) => {
                    state.drawState = drawState;
                });
            },
            markPendingMoveAck() {
                set((state) => {
                    state.isPendingMoveAck = true;
                });
            },

            teleportToMove(number) {
                const { positionHistory, latestMoveOptions: latestLegalMoves } =
                    get();
                if (number < 0 || number >= positionHistory.length) return;

                set((state) => {
                    state.viewingMoveNumber = number;
                });

                const isLatestPosition = number === positionHistory.length - 1;
                return {
                    pieces: positionHistory[number].pieces,
                    moveOptions: isLatestPosition
                        ? latestLegalMoves
                        : createMoveOptions(),
                };
            },

            shiftMoveViewBy(amount) {
                const { teleportToMove, viewingMoveNumber } = get();
                return teleportToMove(viewingMoveNumber + amount);
            },

            teleportToLastMove() {
                const { positionHistory, teleportToMove } = get();
                const lastIndex = positionHistory.length - 1;
                if (lastIndex < 0) throw new Error("positionHistory is empty");
                return teleportToMove(lastIndex)!;
            },

            endGame(resultData) {
                set((state) => {
                    if (
                        state.whitePlayer.rating &&
                        resultData.whiteRatingChange
                    )
                        state.whitePlayer.rating +=
                            resultData.whiteRatingChange;
                    if (
                        state.blackPlayer.rating &&
                        resultData.blackRatingChange
                    )
                        state.blackPlayer.rating +=
                            resultData.blackRatingChange;

                    state.latestMoveOptions = createMoveOptions();
                    state.resultData = resultData;
                });
            },
            resetState(initState) {
                set(() => ({ ...store.getInitialState(), ...initState }));
            },
        })),
        shallow,
    );
}
