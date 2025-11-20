import { GameColor, PieceType } from "@/lib/apiClient";
import { LogicalPoint, StrPoint } from "@/features/point/types";

export type PieceMap = Map<PieceID, Piece>;
export type LegalMoveMap = Map<StrPoint, Move[]>;

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
    moveKey: string;

    triggers: LogicalPoint[];
    captures: LogicalPoint[];
    intermediates: IntermediateSquare[];
    sideEffects: MoveSideEffect[];
    pieceSpawns: Piece[];
    promotesTo: PieceType | null;
}

export type MinimalMove = Partial<Move> & {
    from: LogicalPoint;
    to: LogicalPoint;
};

export interface BoardState {
    pieces: PieceMap;
    moveOptions: ProcessedMoveOptions;
    casuedByMove?: Move;
}

export interface MoveSideEffect {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface IntermediateSquare {
    position: LogicalPoint;
    isCapture: boolean;
}

export type PieceID = string;

export interface AnimationStep {
    newPieces: PieceMap;
    movedPieceIds: PieceID[];
    initialSpawnPositions?: PieceMap;
    isCapture: boolean;
}

export interface MoveAnimation {
    steps: AnimationStep[];
    removedPieceIds: Iterable<PieceID>;
}

export interface GameReplay {
    startingFen: string;
    moves: MinimalMove[];
}
