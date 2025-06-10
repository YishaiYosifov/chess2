import { GameColor, User } from "../lib/apiClient";

export enum GameResult {
    White = "white",
    Black = "black",
    Draw = "draw",
}

export enum Variant {
    FogOfWar = "fog of war",
    Anarchy = "anarchy",
    Chss = "chss",
}

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
    through: Point[];
    to: Point;

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

export const enum WSEventIn {
    Notification = "notification",
    GameStart = "game_start",
}

export const enum WSEventOut {
    Move = "move",
    Resign = "resign",
}

interface GameStart {
    token: string;
}

export interface WSInEventMessageMap {
    [WSEventIn.Notification]: null;
    [WSEventIn.GameStart]: GameStart;
}

interface OutgoingMove {
    origin: Point;
    destination: Point;
}

export interface WSOutEventMessageMap {
    [WSEventOut.Move]: OutgoingMove;
    [WSEventOut.Resign]: null;
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
