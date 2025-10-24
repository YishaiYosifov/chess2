import { BoardState, Move, PieceMap } from "@/features/chessboard/lib/types";

export interface Position {
    move?: Move;
    san?: string;
    pieces: PieceMap;
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
