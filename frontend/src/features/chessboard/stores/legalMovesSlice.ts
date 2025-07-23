import { LegalMoveMap, PieceID, Point } from "@/types/tempModels";
import { StateCreator } from "zustand";
import { ChessboardState } from "./chessboardStore";
import { pointToStr } from "@/lib/utils/pointUtils";

export interface LegalMovesSliceProps {
    legalMoves: LegalMoveMap;
    hasForcedMoves: boolean;
}

export interface LegalMovesSlice {
    legalMoves: LegalMoveMap;
    highlightedLegalMoves: Point[];
    hasForcedMoves: boolean;

    showLegalMoves(pieceId: PieceID): void;
    flashLegalMoves(): void;

    setLegalMoves(legalMoves: LegalMoveMap, hasForcedMoves: boolean): void;
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
            const { legalMoves, pieces } = get();
            const piece = pieces.get(pieceId);
            if (!piece) {
                console.warn(
                    `Cannot show legal moves, no piece was found with id ${pieceId}`,
                );
                return;
            }

            const positionStr = pointToStr(piece.position);
            const moves = legalMoves.get(positionStr);

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
            const { legalMoves, logicalPointToViewPoint, flashOverlay } = get();

            for (const movesPerPoint of legalMoves.values()) {
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

        setLegalMoves(legalMoves: LegalMoveMap, hasForcedMoves: boolean): void {
            set((state) => {
                state.legalMoves = legalMoves;
                state.hasForcedMoves = hasForcedMoves;
            });
        },
    });
}
