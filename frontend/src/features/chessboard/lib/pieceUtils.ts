import { PieceID } from "./types";

export function createPieceId(): PieceID {
    return crypto.randomUUID();
}
