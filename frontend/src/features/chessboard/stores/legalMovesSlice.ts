import {
    LogicalPoint,
    PieceID,
    ProcessedMoveOptions,
} from "@/types/tempModels";
import { StateCreator } from "zustand";
import { ChessboardState } from "./chessboardStore";
import { pointToStr } from "@/lib/utils/pointUtils";

export interface LegalMovesSliceProps {
    moveOptions: ProcessedMoveOptions;
}

export interface LegalMovesSlice {
    moveOptions: ProcessedMoveOptions;
    highlightedLegalMoves: LogicalPoint[];

    showLegalMoves(pieceId: PieceID): void;
    flashLegalMoves(): void;

    setLegalMoves(moveOptions: ProcessedMoveOptions): void;
}

export function createLegalMovesSlice(
    initState: LegalMovesSliceProps,
): StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    LegalMovesSlice
> {
    return (set, get) => ({
        ...initState,
        highlightedLegalMoves: [],

        /**
         * Highlights the legal moves available for the specified piece.
         * Updates the state to reflect these highlighted moves
         *
         * @param pieceId - The ID of the piece for which to show legal moves.
         */
        showLegalMoves(pieceId: PieceID): void {
            const { moveOptions, pieces } = get();
            const piece = pieces.get(pieceId);
            if (!piece) {
                console.warn(
                    `Cannot show legal moves, no piece was found with id ${pieceId}`,
                );
                return;
            }

            const positionStr = pointToStr(piece.position);
            const moves = moveOptions.legalMoves.get(positionStr);

            const toHighlightPoints = moves
                ? [
                      ...moves.map((m) => m.to),
                      ...moves.map((m) => m.triggers).flat(),
                  ]
                : [];

            set((state) => {
                state.highlightedLegalMoves = toHighlightPoints;
            });
        },

        flashLegalMoves(): void {
            const { moveOptions, logicalPointToViewPoint, flashOverlay } =
                get();

            for (const movesPerPoint of moveOptions.legalMoves.values()) {
                for (const move of movesPerPoint) {
                    const from = logicalPointToViewPoint(move.from);
                    const to = logicalPointToViewPoint(move.to);
                    flashOverlay({
                        from: from,
                        to: to,
                        color: "red",
                    });
                }
            }
        },

        setLegalMoves(moveOptions): void {
            set((state) => {
                state.moveOptions = moveOptions;
            });
        },
    });
}
