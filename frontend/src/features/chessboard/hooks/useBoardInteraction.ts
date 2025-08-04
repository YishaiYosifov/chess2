import { useCallback, useEffect, useRef, useState } from "react";
import { InteractionInfo } from "../stores/interactionSlice";
import { useChessboardStore } from "./useChessboard";
import { screenPoint } from "@/lib/utils/pointUtils";
import { ScreenPoint } from "@/features/point/types";
import { MaybePromise } from "@/types/types";

export default function useBoardInteraction({
    shouldStartDrag,

    onDragStart,
    onDragMove,
    onDragEnd,

    onPress,
    onClick,
}: {
    shouldStartDrag: (info: InteractionInfo) => MaybePromise<boolean>;

    onDragStart?: (point: ScreenPoint) => MaybePromise<void>;
    onDragMove?: (point: ScreenPoint) => MaybePromise<void>;
    onDragEnd?: (point: ScreenPoint) => MaybePromise<void>;

    onPress?: (info: InteractionInfo) => MaybePromise<void>;
    onClick?: (info: InteractionInfo) => MaybePromise<void>;
}): boolean {
    const callbacksRef = useRef({
        shouldStartDrag,
        onDragStart,
        onDragMove,
        onDragEnd,
        onPress,
        onClick,
    });

    useEffect(() => {
        callbacksRef.current = {
            shouldStartDrag,
            onDragStart,
            onDragMove,
            onDragEnd,
            onPress,
            onClick,
        };
    }, [shouldStartDrag, onDragStart, onDragMove, onDragEnd, onPress, onClick]);

    const [isDragging, setIsDragging] = useState(false);

    const isDraggingRef = useRef(false);
    const hasHandledPointerUpRef = useRef(false);

    const { pointerDownEvent, dragStartQuery, pointerUpEvent } =
        useChessboardStore((x) => ({
            pointerDownEvent: x.pointerDownEvent,
            dragStartQuery: x.dragStartQuery,
            pointerUpEvent: x.pointerUpEvent,
        }));

    const startDragging = useCallback(async (startFrom: ScreenPoint) => {
        setIsDragging(true);
        await callbacksRef.current.onDragStart?.(startFrom);
        isDraggingRef.current = true;

        let animationFrameId: number | null = null;
        let lastMouseX = startFrom.x;
        let lastMouseY = startFrom.y;

        async function emitDrag(): Promise<void> {
            if (!isDraggingRef.current) return;

            await callbacksRef.current.onDragMove?.(
                screenPoint({
                    x: lastMouseX,
                    y: lastMouseY,
                }),
            );
            animationFrameId = null;
        }

        function handleMove(event: PointerEvent) {
            lastMouseX = event.clientX;
            lastMouseY = event.clientY;

            if (animationFrameId === null) {
                animationFrameId = requestAnimationFrame(emitDrag);
            }
        }

        async function stopDragging(event: PointerEvent) {
            if (hasHandledPointerUpRef.current || !isDraggingRef.current)
                return;
            isDraggingRef.current = false;

            await callbacksRef.current.onDragEnd?.(
                screenPoint({
                    x: event.clientX,
                    y: event.clientY,
                }),
            );
            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);

            setIsDragging(false);
            hasHandledPointerUpRef.current = true;
        }

        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);
        await emitDrag();
    }, []);

    useEffect(() => {
        async function pointerDownHandler(
            info: InteractionInfo,
        ): Promise<void> {
            hasHandledPointerUpRef.current = false;
            await callbacksRef.current.onPress?.(info);
        }

        async function shouldStartDragHandler(
            info: InteractionInfo,
        ): Promise<boolean> {
            const shouldStart =
                await callbacksRef.current?.shouldStartDrag(info);
            if (shouldStart) await startDragging(info.point);

            return shouldStart;
        }

        async function pointerUpHandler(info: InteractionInfo): Promise<void> {
            if (isDraggingRef.current || hasHandledPointerUpRef.current) return;

            await callbacksRef.current.onClick?.(info);
            hasHandledPointerUpRef.current = true;
        }

        pointerDownEvent.subscribe(pointerDownHandler);
        dragStartQuery.subscribe(shouldStartDragHandler);
        pointerUpEvent.subscribe(pointerUpHandler);

        return () => {
            pointerDownEvent.unsubscribe(pointerDownHandler);
            dragStartQuery.unsubscribe(shouldStartDragHandler);
            pointerUpEvent.unsubscribe(pointerUpHandler);
        };
    }, [pointerUpEvent, dragStartQuery, pointerDownEvent, startDragging]);

    return isDragging;
}
