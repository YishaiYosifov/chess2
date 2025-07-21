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
    listenToPointerDown(handler: (info: InteractionInfo) => void): void;
    disregardPointerDown(handler: (info: InteractionInfo) => void): void;

    onPointerUpListeners: Set<(info: InteractionInfo) => void>;
    listenToPointerUp(handler: (info: InteractionInfo) => void): void;
    disregardPointerUp(handler: (info: InteractionInfo) => void): void;

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
    listenToPointerDown(handler) {
        set((state) => {
            state.onPointerDownListeners.add(handler);
        });
    },
    disregardPointerDown(handler) {
        set((state) => {
            state.onPointerDownListeners.delete(handler);
        });
    },

    onPointerUpListeners: new Set(),
    listenToPointerUp(handler) {
        set((state) => {
            state.onPointerUpListeners.add(handler);
        });
    },
    disregardPointerUp(handler) {
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
