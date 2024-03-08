import { StateCreator } from "zustand";
import type { State } from "../store";

const enum WSEvent {
    Notification = "notification",
    GameStart = "game_start",
}

interface Notification {}
interface GameStart {}

type WSEventMessageMap = {
    [WSEvent.Notification]: Notification;
    [WSEvent.GameStart]: GameStart;
};

type WSMessage<E extends WSEvent> = {
    event: E;
} & WSEventMessageMap[E];

export interface WSSlice {
    lastMessage?: WSMessage<any>;
}

export const createWSSlice: StateCreator<
    State,
    [],
    [],
    WSSlice
> = (): WSSlice => ({});
