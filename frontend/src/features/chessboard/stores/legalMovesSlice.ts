import { LogicalPoint } from "@/features/point/types";
import { PieceID, ProcessedMoveOptions } from "../lib/types";
import { Move } from "../lib/types";
import { StrPoint } from "@/features/point/types";
import { Piece } from "../lib/types";
import { StateCreator } from "zustand";
import { ChessboardStore } from "./chessboardStore";
import { pointToStr } from "@/features/point/pointUtils";
import { PieceType } from "@/lib/apiClient";

export interface LegalMovesSliceProps {
    moveOptions: ProcessedMoveOptions;
}

export interface LegalMovesSlice {
    moveOptions: ProcessedMoveOptions;
    highlightedLegalMoves: LogicalPoint[];

    getLegalMove(
        origin: LogicalPoint,
        dest: LogicalPoint,
        pieceId: PieceID,
        piece: Piece,
    ): Promise<Move | undefined>;

    showLegalMoves(piece: Piece): void;
    flashLegalMoves(): void;

    setLegalMoves(moveOptions: ProcessedMoveOptions): void;
}

export function createLegalMovesSlice(
    initState: LegalMovesSliceProps,
): StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    LegalMovesSlice
> {
    return (set, get) => ({
        ...initState,
        highlightedLegalMoves: [],

        async getLegalMove(origin, dest, pieceId, piece) {
            const { moveOptions, promptPromotion, disambiguateDestination } =
                get();

            const movesFromOrigin = moveOptions.legalMoves.get(
                pointToStr(origin),
            );
            if (!movesFromOrigin) return;

            const movesToDest = await disambiguateDestination(
                dest,
                movesFromOrigin,
                pieceId,
                piece,
            );
            if (!movesToDest) return;

            if (movesToDest.length === 0) return;
            else if (movesToDest.length === 1) return movesToDest[0];

            const availablePromotions = new Map<PieceType | null, Move>();
            for (const move of movesToDest) {
                availablePromotions.set(move.promotesTo, move);
            }

            const promoteTo = await promptPromotion({
                at: dest,
                pieces: [...availablePromotions.keys()],
                piece,
            });
            return availablePromotions.get(promoteTo);
        },

        /**
         * Highlights the legal moves available for the specified piece.
         * Updates the state to reflect these highlighted moves
         *
         * @param pieceId - The ID of the piece for which to show legal moves.
         */
        showLegalMoves(piece) {
            const { moveOptions } = get();

            const positionStr = pointToStr(piece.position);
            const moves = moveOptions.legalMoves.get(positionStr) ?? [];

            const toHighlightPoints = new Map<StrPoint, LogicalPoint>();
            for (const move of moves) {
                for (const trigger of move.triggers) {
                    toHighlightPoints.set(pointToStr(trigger), trigger);
                }
                if (move.intermediates.length == 0) {
                    toHighlightPoints.set(pointToStr(move.to), move.to);
                } else {
                    toHighlightPoints.set(
                        pointToStr(move.intermediates[0]),
                        move.intermediates[0],
                    );
                }
            }

            set((state) => {
                state.highlightedLegalMoves = Array.from(
                    toHighlightPoints.values(),
                );
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
