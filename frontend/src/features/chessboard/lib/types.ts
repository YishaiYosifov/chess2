import { GameColor, PieceType } from "@/lib/apiClient";
import { LogicalPoint, StrPoint } from "@/features/point/types";

export type PieceMap = Map<PieceID, Piece>;
export type LegalMoveMap = Map<StrPoint, Move[]>;

export interface MoveKey {
    from: LogicalPoint;
    to: LogicalPoint;
    promotesTo: PieceType | null;
}

export interface Piece {
    type: PieceType;
    color: GameColor | null;
    position: LogicalPoint;
}

export interface ProcessedMoveOptions {
    legalMoves: LegalMoveMap;
    hasForcedMoves: boolean;
}

export interface Move {
    from: LogicalPoint;
    to: LogicalPoint;

    triggers: LogicalPoint[];
    captures: LogicalPoint[];
    intermediates: LogicalPoint[];
    sideEffects: MoveSideEffect[];
    promotesTo: PieceType | null;
}

export interface BoardState {
    pieces: PieceMap;
    moveOptions: ProcessedMoveOptions;
    casuedByMove?: Move;
}

export interface MoveSideEffect {
    from: LogicalPoint;
    to: LogicalPoint;
}

export type PieceID = `${number}`;
