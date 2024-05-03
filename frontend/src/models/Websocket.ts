export const enum WSEvent {
    Notification = "notification",
    GameStart = "game_start",
}
export interface Notification {
    someValue: boolean;
}
export interface GameStart {
    token: string;
}

export interface WSEventMessageMap {
    [WSEvent.Notification]: Notification;
    [WSEvent.GameStart]: GameStart;
}
