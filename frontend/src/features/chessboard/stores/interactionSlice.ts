import { StateCreator } from "zustand";

import type { ChessboardState } from "./chessboardStore";
import { Point } from "@/types/tempModels";
import React from "react";
import { enableMapSet } from "immer";

export interface InteractionInfo {
    point: Point;
    button: number;
}

export interface InteractionSlice {
    interaction: InteractionInfo | null;

    onPointerDownListeners: Set<(info: InteractionInfo) => void>;
    subscribePointerDown(handler: (info: InteractionInfo) => void): void;
    unsubscribePointerDown(handler: (info: InteractionInfo) => void): void;

    onPointerUpListeners: Set<(info: InteractionInfo) => void>;
    subscribePointerUp(handler: (info: InteractionInfo) => void): void;
    unsubscribePointerUp(handler: (info: InteractionInfo) => void): void;

    onPointerDown(event: React.MouseEvent): void;
    onPointerUp(event: React.MouseEvent): void;
}

enableMapSet();
export const createInteractionSlice: StateCreator<
    ChessboardState,
    [["zustand/immer", never], never],
    [],
    InteractionSlice
> = (set, get) => ({
    interaction: null,

    onPointerDownListeners: new Set(),
    subscribePointerDown(handler) {
        set((state) => {
            state.onPointerDownListeners.add(handler);
        });
    },
    unsubscribePointerDown(handler) {
        set((state) => {
            state.onPointerDownListeners.delete(handler);
        });
    },

    onPointerUpListeners: new Set(),
    subscribePointerUp(handler) {
        set((state) => {
            state.onPointerUpListeners.add(handler);
        });
    },
    unsubscribePointerUp(handler) {
        set((state) => {
            state.onPointerUpListeners.delete(handler);
        });
    },

    onPointerDown(event: React.MouseEvent): void {
        const info: InteractionInfo = {
            point: {
                x: event.clientX,
                y: event.clientY,
            },
            button: event.button,
        };
        set((store) => {
            store.interaction = info;
        });

        const { onPointerDownListeners } = get();
        for (const listener of onPointerDownListeners) {
            listener(info);
        }
    },

    onPointerUp(event: React.MouseEvent) {
        set((store) => {
            store.interaction = null;
        });

        const info: InteractionInfo = {
            point: {
                x: event.clientX,
                y: event.clientY,
            },
            button: event.button,
        };

        const { onPointerUpListeners } = get();
        for (const listener of onPointerUpListeners) {
            listener(info);
        }
    },
});
