import { Point } from "./Game";

export const enum WSEventIn {
    Notification = "notification",
    GameStart = "game_start",
}

export const enum WSEventOut {
    Move = "move",
    Resign = "resign",
}

interface GameStart {
    token: string;
}

export interface WSInEventMessageMap {
    [WSEventIn.Notification]: null;
    [WSEventIn.GameStart]: GameStart;
}

interface OutgoingMove {
    origin: Point;
    destination: Point;
}

export interface WSOutEventMessageMap {
    [WSEventOut.Move]: OutgoingMove;
    [WSEventOut.Resign]: null;
}
