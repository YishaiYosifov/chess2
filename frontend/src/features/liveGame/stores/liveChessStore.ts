import { createWithEqualityFn } from "zustand/traditional";
import { immer } from "zustand/middleware/immer";

import { Clocks, GameColor, GamePlayer, GameResultData } from "@/lib/apiClient";
import { shallow } from "zustand/shallow";
import { enableMapSet } from "immer";
import { BoardState, Position, ProcessedMoveOptions } from "@/types/tempModels";
import { createMoveOptions } from "@/features/chessboard/lib/moveOptions";

export interface LiveChessStoreProps {
    gameToken: string;
    positionHistory: Position[];
    viewingMoveNumber: number;
    latestMoveOptions: ProcessedMoveOptions;

    sideToMove: GameColor;
    playerColor: GameColor;
    whitePlayer: GamePlayer;
    blackPlayer: GamePlayer;

    clocks: Clocks;
    resultData: GameResultData | null;
}

export interface LiveChessStore extends LiveChessStoreProps {
    receiveMove(
        position: Position,
        clocks: Clocks,
        sideToMove: GameColor,
    ): void;
    receiveLegalMoves(moveOptions: ProcessedMoveOptions): void;

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

            receiveMove(position, clocks, sideToMove) {
                const { viewingMoveNumber, positionHistory } = get();
                set((state) => {
                    if (viewingMoveNumber === positionHistory.length - 1)
                        state.viewingMoveNumber++;

                    state.positionHistory.push(position);
                    state.clocks = clocks;
                    state.sideToMove = sideToMove;
                });
            },
            receiveLegalMoves(moveOptions) {
                set((state) => {
                    state.latestMoveOptions = moveOptions;
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
