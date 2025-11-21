import { MinimalMove, Move } from "./types";

export default function expandMinimalMove(minimalMove: MinimalMove): Move {
    return {
        moveKey: "",
        triggers: [],
        captures: [],
        intermediates: [],
        sideEffects: [],
        pieceSpawns: [],
        promotesTo: null,
        specialMoveType: null,
        ...minimalMove,
    };
}
