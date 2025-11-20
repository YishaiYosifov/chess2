import BoardPieces from "@/features/chessboard/lib/boardPieces";
import { BoardState, Move } from "@/features/chessboard/lib/types";

export interface Position {
    move?: Move;
    san?: string;
    pieces: BoardPieces;
    clocks: ClockSnapshot;
}

export interface HistoryStep {
    state: BoardState;
    isOneStepForward: boolean;
}

export interface ClockSnapshot {
    whiteClock: number;
    blackClock: number;
}
