import { GameColor } from "../lib/apiClient";

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
    UNDERAGEPAWN = "d",
}

export type PieceID = `${number}`;

type Brand<T, B extends symbol> = T & { readonly [brand in B]: true };

export interface Point {
    x: number;
    y: number;
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
declare const logicalPointBrand: unique symbol;
// eslint-disable-next-line @typescript-eslint/no-unused-vars
declare const viewPointBrand: unique symbol;
// eslint-disable-next-line @typescript-eslint/no-unused-vars
declare const screenPointBrand: unique symbol;

export type ScreenPoint = Brand<Point, typeof screenPointBrand>;
export type ViewPoint = Brand<Point, typeof viewPointBrand>;
export type LogicalPoint = Brand<Point, typeof logicalPointBrand>;

export type StrPoint = `${number},${number}`;

export interface Piece {
    type: PieceType;
    color: GameColor;
    position: LogicalPoint;
}

export type PieceMap = Map<PieceID, Piece>;
export type LegalMoveMap = Map<StrPoint, Move[]>;

export interface ProcessedMoveOptions {
    legalMoves: LegalMoveMap;
    hasForcedMoves: boolean;
}

export interface Move {
    from: LogicalPoint;
    to: LogicalPoint;

    triggers: LogicalPoint[];
    captures: LogicalPoint[];
    sideEffects: MoveSideEffect[];
}

export interface MoveSideEffect {
    from: LogicalPoint;
    to: LogicalPoint;
}

export interface Position {
    san?: string;
    pieces: PieceMap;
    clocks: ClockSnapshot;
}

export interface BoardState {
    pieces: PieceMap;
    moveOptions: ProcessedMoveOptions;
}

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}

export type MaybePromise<T> = Promise<T> | T;
