import { Piece, PieceID, PieceMap } from "./types";

export function pieceMapFromPieces(...pieces: Piece[]): PieceMap {
    const pieceMap: PieceMap = new Map();
    for (const piece of pieces) {
        pieceMap.set(createPieceId(), piece);
    }
    return pieceMap;
}

export function createPieceId(): PieceID {
    return crypto.randomUUID();
}
