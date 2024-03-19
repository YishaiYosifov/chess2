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

export type Point = [x: number, y: number];

export interface Piece {
    position: Point;
    pieceType: PieceType;
    color: Color;
}

export type PieceMap = Map<string, Piece>;

export enum Variant {
    Anarchy = "anarchy",
    FogOfWar = "fog of war",
    Chss = "chss",
}
