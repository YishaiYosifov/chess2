import { GameColor, GameResult, User } from "../lib/apiClient";

export enum PieceType {
    KING = "k",
    QUEEN = "q",
    ROOK = "r",
    KNOOK = "n",
    XOOK = "x",
    ANTIQUEEN = "a",
    ARCHBISHOP = "c",
    BISHOP = "b",
    HORSEY = "h",
    PAWN = "p",
    CHILDPAWN = "d",
}

export type PieceID = `${number}`;

export interface Point {
    x: number;
    y: number;
}
export type StrPoint = `${number},${number}`;

export interface Piece {
    type: PieceType;
    color: GameColor;
    position: Point;
}

export type PieceMap = Map<PieceID, Piece>;
export type LegalMoveMap = Map<StrPoint, Move[]>;

export interface Move {
    from: Point;
    to: Point;

    triggers: Point[];
    captures: Point[];
    sideEffects: Move[];
}

export interface FinishedGame {
    token: string;

    userWhite?: User;
    userBlack?: User;

    timeControl: number;
    increment: number;

    results: GameResult;
    createdAt: number;
}

export interface Rating {
    elo: number;
    achievedAt: number;
}

export interface RatingOverview {
    max: number;
    current: number;
    history: Array<Rating>;
}
