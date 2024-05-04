export const enum WSEventIn {
    Notification = "notification",
    GameStart = "game_start",
}

export const enum WSEventOut {
    Move = "move",
    Resign = "resign",
}

export interface Notification {
    someValue: boolean;
}

export interface GameStart {
    token: string;
}

export interface WSInEventMessageMap {
    [WSEventIn.Notification]: Notification;
    [WSEventIn.GameStart]: GameStart;
}

export interface WSOutEventMessageMap {
    [WSEventOut.Move]: null;
    [WSEventOut.Resign]: null;
}
