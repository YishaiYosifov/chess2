import { immerable } from "immer";

import { LogicalPoint, StrPoint } from "@/features/point/types";
import { Move, Piece, PieceID } from "./types";
import { pointToStr } from "@/features/point/pointUtils";

interface SinglePieceMove {
    pieceId: PieceID;
    from: LogicalPoint;
    to: LogicalPoint;
}

interface GatheredMoves {
    pieceMoves: SinglePieceMove[];
    movedPieceIds: Set<PieceID>;
}

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

    playMove(move: Move): {
        movedPieceIds: PieceID[];
        removedPieceIds: PieceID[];
    } {
        const { pieceMoves, movedPieceIds } = this._gatherMoves(move);
        const removedPieceIds: PieceID[] = [];

        // step 1: remove all captures first
        // so we don't capture any piece that just moved
        for (const capture of move.captures) {
            const capturedPiece = this.getByPosition(capture);
            if (capturedPiece) {
                this.delete(capturedPiece.id);
                removedPieceIds.push(capturedPiece.id);
            }
        }

        // step 2: clear all origin squares of moving pieces
        // this is done before placing pieces to handle swaps correctly
        for (const move of pieceMoves) {
            this._byPosition.delete(pointToStr(move.from));
        }

        // step 3: place all pieces on their final destinations
        for (const move of pieceMoves) {
            this._byPosition.set(pointToStr(move.to), move.pieceId);
            const piece = this._byId.get(move.pieceId);
            if (piece) piece.position = move.to;
        }

        for (const spawn of move.pieceSpawns) {
            this.add(spawn);
            movedPieceIds.add(spawn.id);
        }

        if (move.promotesTo !== null)
            this.getByPosition(move.to)!.type = move.promotesTo;

        return {
            removedPieceIds,
            movedPieceIds: [...movedPieceIds],
        };
    }

    movePiece(pieceId: PieceID, to: LogicalPoint) {
        const piece = this._byId.get(pieceId);
        if (!piece) return;

        this._byPosition.delete(pointToStr(piece.position));
        piece.position = to;

        const newPositionStr = pointToStr(to);
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

    _gatherMoves(move: Move): GatheredMoves {
        const pieceMoves: SinglePieceMove[] = [];
        const movedPieceIds = new Set<PieceID>();

        const mainPieceId = this._byPosition.get(pointToStr(move.from));
        if (mainPieceId) {
            pieceMoves.push({
                pieceId: mainPieceId,
                from: move.from,
                to: move.to,
            });
            movedPieceIds.add(mainPieceId);
        } else {
            console.warn(
                "Could not find piece to move at",
                pointToStr(move.from),
            );
        }

        for (const sideEffect of move.sideEffects) {
            const sideEffectPieceId = this._byPosition.get(
                pointToStr(sideEffect.from),
            );
            if (!sideEffectPieceId) {
                console.warn(
                    "Could not find side effect piece at",
                    pointToStr(sideEffect.from),
                );
                continue;
            }

            pieceMoves.push({
                pieceId: sideEffectPieceId,
                from: sideEffect.from,
                to: sideEffect.to,
            });
            movedPieceIds.add(sideEffectPieceId);
        }

        return { pieceMoves, movedPieceIds };
    }
}
