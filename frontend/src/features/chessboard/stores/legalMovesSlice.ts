import { LogicalPoint } from "@/features/point/types";
import { PieceID, ProcessedMoveOptions } from "../lib/types";
import { Move } from "../lib/types";
import { StrPoint } from "@/features/point/types";
import { Piece } from "../lib/types";
import { StateCreator } from "zustand";
import { ChessboardStore } from "./chessboardStore";
import { pointEquals, pointToStr } from "@/lib/utils/pointUtils";
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
        piece: Piece,
    ): Promise<Move | undefined>;

    showLegalMoves(pieceId: PieceID): void;
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

        async getLegalMove(origin, dest, piece) {
            const { moveOptions, promptPromotion } = get();

            const movesFromOrigin = moveOptions.legalMoves.get(
                pointToStr(origin),
            );
            if (!movesFromOrigin) return;

            const movesToDest = movesFromOrigin?.filter(
                (candidateMove) =>
                    pointEquals(candidateMove.to, dest) ||
                    candidateMove.triggers.some((triggerPoint) =>
                        pointEquals(triggerPoint, dest),
                    ),
            );

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
        showLegalMoves(pieceId) {
            const { moveOptions, pieces } = get();
            const piece = pieces.get(pieceId);
            if (!piece) {
                console.warn(
                    `Cannot show legal moves, no piece was found with id ${pieceId}`,
                );
                return;
            }

            const positionStr = pointToStr(piece.position);
            const moves = moveOptions.legalMoves.get(positionStr) ?? [];

            const toHighlightPoints = new Map<StrPoint, LogicalPoint>();
            for (const move of moves) {
                toHighlightPoints.set(pointToStr(move.to), move.to);
                for (const trigger of move.triggers) {
                    toHighlightPoints.set(pointToStr(trigger), trigger);
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
