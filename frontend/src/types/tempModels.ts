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
    King = "k",
    Queen = "q",
    Rook = "r",
    Knook = "n",
    Xook = "x",
    Antiqueen = "a",
    Archbishop = "c",
    Bishop = "b",
    Horsie = "h",
    Pawn = "p",
    ChildPawn = "d",
}

export type PieceID = `${number}`;

export type Point = [x: number, y: number];
export type StrPoint = `${number},${number}`;

export type LegalMoves = Record<StrPoint, StrPoint[]>;
export type PieceMap = Map<PieceID, Piece>;

export interface Piece {
    position: Point;
    pieceType: PieceType;
    color: GameColor;
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
