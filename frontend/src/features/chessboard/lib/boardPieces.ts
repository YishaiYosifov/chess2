import { immerable } from "immer";

import { LogicalPoint, StrPoint } from "@/features/point/types";
import { Piece, PieceID } from "./types";
import { pointToStr } from "@/features/point/pointUtils";
import { PieceType } from "@/lib/apiClient";

export default class BoardPieces {
    [immerable] = true;

    _byId: Map<PieceID, Piece>;
    _byPosition: Map<StrPoint, PieceID>;

    constructor(copy: BoardPieces | null = null) {
        if (copy === null) {
            this._byId = new Map();
            this._byPosition = new Map();
            return;
        }

        this._byId = new Map(
            [...copy._byId].map(([id, piece]) => {
                return [id, { ...piece }];
            }),
        );
        this._byPosition = new Map(copy._byPosition);
    }

    static fromPieces(...pieces: Piece[]): BoardPieces {
        const boardPieces = new BoardPieces();
        for (const piece of pieces) {
            boardPieces.add(piece);
        }
        return boardPieces;
    }

    getById(pieceId: PieceID): Piece | undefined {
        return this._byId.get(pieceId);
    }

    getByPosition(position: LogicalPoint): Piece | undefined {
        const pieceId = this._byPosition.get(pointToStr(position));
        if (!pieceId) return undefined;

        return this._byId.get(pieceId);
    }

    move(
        pieceId: PieceID,
        newPosition: LogicalPoint,
        promotesTo: PieceType | null = null,
    ): void {
        const piece = this._byId.get(pieceId);
        if (!piece) return;

        this._byPosition.delete(pointToStr(piece.position));

        piece.position = newPosition;
        if (promotesTo !== null) piece.type = promotesTo;

        const newPositionStr = pointToStr(newPosition);
        const inNewPosition = this._byPosition.get(newPositionStr);
        if (inNewPosition) this._byId.delete(inNewPosition);
        this._byPosition.set(newPositionStr, pieceId);
    }

    add(piece: Piece): void {
        this._byId.set(piece.id, { ...piece });
        this._byPosition.set(pointToStr(piece.position), piece.id);
    }

    addAt(piece: Piece, position: LogicalPoint): void {
        const newPiece = { ...piece, position };
        this._byId.set(newPiece.id, newPiece);
        this._byPosition.set(pointToStr(position), newPiece.id);
    }

    delete(pieceId: PieceID): void {
        const piece = this._byId.get(pieceId);
        if (!piece) return;
        this._byId.delete(pieceId);
        this._byPosition.delete(pointToStr(piece.position));
    }

    values(): IterableIterator<Piece> {
        return this._byId.values();
    }

    keys(): IterableIterator<PieceID> {
        return this._byId.keys();
    }

    *[Symbol.iterator](): IterableIterator<Piece> {
        yield* this._byId.values();
    }
}
