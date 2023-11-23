export const USERNAME_EDIT_EVERY = 2419200;

export const BOARD_WIDTH = 10;
export const BOARD_HEIGHT = 10;
export const BOARD_SIZE = BOARD_WIDTH * BOARD_HEIGHT;

export enum Piece {
    King = "king",
    Queen = "queen",
    Rook = "rook",
    Knook = "knook",
    Xook = "xook",
    Antiqueen = "antiqueen",
    Archbishop = "archbishop",
    Bishop = "bishop",
    Horse = "horse",
    Pawn = "pawn",
    ChildPawn = "ChildPawn",
}

export enum Color {
    White = "white",
    Black = "black",
}

export const enum GameResult {
    White = "white",
    Black = "black",
    Draw = "draw",
}

export enum Variant {
    Anarchy = "anarchy",
    FogOfWar = "fog of war",
    Chss = "chss",
}

export const TIME_CONTROLS = [
    { type: "bullet", timeControl: 60, increment: 0 },
    { type: "bullet", timeControl: 60, increment: 1 },
    { type: "bullet", timeControl: 120, increment: 1 },
    { type: "blitz", timeControl: 180, increment: 0 },
    { type: "blitz", timeControl: 180, increment: 2 },
    { type: "blits", timeControl: 300, increment: 0 },
    { type: "rapid", timeControl: 600, increment: 0 },
    { type: "rapid", timeControl: 900, increment: 10 },
    { type: "rapid", timeControl: 1800, increment: 0 },
];

export interface PieceData {
    piece: Piece;
    color: Color;
}

export const defaultChessboard: Record<number, PieceData> = {
    0: { piece: Piece.Rook, color: Color.White },
    1: { piece: Piece.Horse, color: Color.White },
    2: { piece: Piece.Knook, color: Color.White },
    3: { piece: Piece.Xook, color: Color.White },
    4: { piece: Piece.Queen, color: Color.White },
    5: { piece: Piece.King, color: Color.White },
    6: { piece: Piece.Bishop, color: Color.White },
    7: { piece: Piece.Antiqueen, color: Color.White },
    8: { piece: Piece.Horse, color: Color.White },
    9: { piece: Piece.Rook, color: Color.White },
    10: { piece: Piece.ChildPawn, color: Color.White },
    11: { piece: Piece.ChildPawn, color: Color.White },
    12: { piece: Piece.Pawn, color: Color.White },
    13: { piece: Piece.ChildPawn, color: Color.White },
    14: { piece: Piece.Pawn, color: Color.White },
    15: { piece: Piece.Pawn, color: Color.White },
    16: { piece: Piece.ChildPawn, color: Color.White },
    17: { piece: Piece.Pawn, color: Color.White },
    18: { piece: Piece.ChildPawn, color: Color.White },
    19: { piece: Piece.ChildPawn, color: Color.White },
    20: { piece: Piece.Archbishop, color: Color.White },
    29: { piece: Piece.Archbishop, color: Color.White },
    70: { piece: Piece.Archbishop, color: Color.Black },
    79: { piece: Piece.Archbishop, color: Color.Black },
    80: { piece: Piece.ChildPawn, color: Color.Black },
    81: { piece: Piece.ChildPawn, color: Color.Black },
    82: { piece: Piece.Pawn, color: Color.Black },
    83: { piece: Piece.ChildPawn, color: Color.Black },
    84: { piece: Piece.Pawn, color: Color.Black },
    85: { piece: Piece.Pawn, color: Color.Black },
    86: { piece: Piece.ChildPawn, color: Color.Black },
    87: { piece: Piece.Pawn, color: Color.Black },
    88: { piece: Piece.ChildPawn, color: Color.Black },
    89: { piece: Piece.ChildPawn, color: Color.Black },
    90: { piece: Piece.Rook, color: Color.Black },
    91: { piece: Piece.Horse, color: Color.Black },
    92: { piece: Piece.Knook, color: Color.Black },
    93: { piece: Piece.Xook, color: Color.Black },
    94: { piece: Piece.Queen, color: Color.Black },
    95: { piece: Piece.King, color: Color.Black },
    96: { piece: Piece.Bishop, color: Color.Black },
    97: { piece: Piece.Antiqueen, color: Color.Black },
    98: { piece: Piece.Horse, color: Color.Black },
    99: { piece: Piece.Rook, color: Color.Black },
};
