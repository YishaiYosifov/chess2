export type Point = [x: number, y: number];

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

export enum Color {
    White = "white",
    Black = "black",
}

export interface PieceInfo {
    pieceType: PieceType;
    color: Color;
}

export type ChessBoard = [Point, PieceInfo][];

export enum Variant {
    Anarchy = "anarchy",
    FogOfWar = "fog of war",
    Chss = "chss",
}
