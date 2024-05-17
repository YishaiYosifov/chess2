import { Point } from "./Game";

export const enum WSEventIn {
    Notification = "notification",
    GameStart = "game_start",
}

export const enum WSEventOut {
    Move = "move",
    Resign = "resign",
}

export interface WSInEventMessageMap {
    [WSEventIn.Notification]: null;
    [WSEventIn.GameStart]: null;
}

export interface OutgoingMove {
    origin: Point;
    destination: Point;
}

export interface WSOutEventMessageMap {
    [WSEventOut.Move]: OutgoingMove;
    [WSEventOut.Resign]: null;
}
