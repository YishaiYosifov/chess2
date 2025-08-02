import {
    PieceMap,
    ProcessedMoveOptions,
} from "@/features/chessboard/lib/types";

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
