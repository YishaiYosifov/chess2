import { UnauthedProfileOut } from "./User";

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

export enum Color {
    White = "white",
    Black = "black",
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
    color: Color;
}

export interface LiveGame {
    token: string;

    playerWhite: Player;
    playerBlack: Player;

    turnPlayerId: number;
    fen: string;

    legalMoves: LegalMoves;
}

export interface Player {
    playerId: number;
    user: UnauthedProfileOut;

    color: Color;
    timeRemaining: number;
}

export interface GameSettings {
    variant: Variant;
    timeControl: number;
    increment: number;
}

export interface FinishedGame {
    token: string;

    userWhite: UnauthedProfileOut | null;
    userBlack: UnauthedProfileOut | null;

    variant: Variant;
    timeControl: number;
    increment: number;

    results: GameResult;
    createdAt: Date;
}
