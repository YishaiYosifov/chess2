import { GameColor, PieceType } from "@/lib/apiClient";
import { LogicalPoint, StrPoint } from "@/features/point/types";
import BoardPieces from "./boardPieces";

export type LegalMoveMap = Map<StrPoint, Move[]>;

export interface Piece {
    id: PieceID;
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
    pieces: BoardPieces;
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
    newPieces: BoardPieces;
    movedPieceIds: PieceID[];
    initialSpawnPositions?: BoardPieces;
    isCapture: boolean;
}

export interface MoveAnimation {
    steps: AnimationStep[];
    removedPieceIds: PieceID[];
}

export interface GameReplay {
    startingFen: string;
    moves: MinimalMove[];
}
