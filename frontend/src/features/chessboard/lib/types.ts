import { GameColor, PieceType, SpecialMoveType } from "@/lib/apiClient";
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
    specialMoveType: SpecialMoveType | null;
}

export type MinimalMove = Partial<Move> & {
    from: LogicalPoint;
    to: LogicalPoint;
};

export interface BoardState {
    pieces: BoardPieces;
    moveOptions: ProcessedMoveOptions;
    moveThatProducedPosition?: Move;
    moveFromPreviousViewedPosition?: Move;
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

export interface MoveBounds {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface AnimationStep {
    newPieces: BoardPieces;
    movedPieceIds: PieceID[];

    initialSpawnPositions?: BoardPieces;

    moveBounds?: MoveBounds;
    specialMoveType?: SpecialMoveType | null;
    isCapture?: boolean;
    isPromotion?: boolean;
}

export interface MoveAnimation {
    steps: AnimationStep[];
    removedPieceIds: PieceID[];
}

export interface GameReplay {
    startingFen: string;
    moves: MinimalMove[];
}
