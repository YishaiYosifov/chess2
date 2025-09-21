import { StateCreator } from "zustand";
import { enableMapSet } from "immer";
import React from "react";

import type { ChessboardStore } from "./chessboardStore";
import { ScreenPoint } from "@/features/point/types";
import { EventBus } from "@/lib/eventBus";
import { screenPoint } from "@/features/point/pointUtils";

export interface InteractionInfo {
    point: ScreenPoint;
    button: number;
}

export interface InteractionSlice {
    interaction: InteractionInfo | null;

    pointerDownEvent: EventBus<[InteractionInfo], void>;
    dragStartQuery: EventBus<[InteractionInfo], boolean>;
    pointerUpEvent: EventBus<[InteractionInfo], void>;

    onPointerDown(event: React.MouseEvent): Promise<void>;
    onPointerUp(event: React.MouseEvent): Promise<void>;

    evaluateDragStart(info: InteractionInfo): Promise<void>;
}

enableMapSet();
export const createInteractionSlice: StateCreator<
    ChessboardStore,
    [["zustand/immer", never], never],
    [],
    InteractionSlice
> = (set, get) => ({
    interaction: null,

    pointerDownEvent: new EventBus(),
    dragStartQuery: new EventBus(),
    pointerUpEvent: new EventBus(),

    async onPointerDown(event: React.MouseEvent): Promise<void> {
        const info: InteractionInfo = {
            point: screenPoint({
                x: event.clientX,
                y: event.clientY,
            }),
            button: event.button,
        };
        set((store) => {
            store.interaction = info;
        });

        const { pointerDownEvent, evaluateDragStart } = get();
        await pointerDownEvent.emit(info);
        await evaluateDragStart(info);
    },

    async onPointerUp(event: React.MouseEvent) {
        set((store) => {
            store.interaction = null;
        });

        const info: InteractionInfo = {
            point: screenPoint({
                x: event.clientX,
                y: event.clientY,
            }),
            button: event.button,
        };

        const { pointerUpEvent } = get();
        pointerUpEvent.emit(info);
    },

    async evaluateDragStart(info) {
        const { dragStartQuery } = get();
        await dragStartQuery.emitUntilTruthy(info);
    },
});
