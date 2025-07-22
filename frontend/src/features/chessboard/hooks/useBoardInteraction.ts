import { useCallback, useEffect, useRef, useState } from "react";
import { InteractionInfo } from "../stores/interactionSlice";
import { Point } from "@/types/tempModels";
import { useChessboardStore } from "./useChessboard";

export default function useBoardInteraction({
    shouldStartDrag,

    onDragStart,
    onDragMove,
    onDragEnd,

    onPress,
    onClick,
}: {
    shouldStartDrag: (info: InteractionInfo) => boolean;

    onDragStart?: (point: Point) => void;
    onDragMove?: (point: Point) => void;
    onDragEnd?: (point: Point) => void;

    onPress?: (info: InteractionInfo) => void;
    onClick?: (info: InteractionInfo) => void;
}): boolean {
    const callbacksRef = useRef({
        onDragStart,
        onDragMove,
        onDragEnd,
        onPress,
        onClick,
    });

    useEffect(() => {
        callbacksRef.current = {
            onDragStart,
            onDragMove,
            onDragEnd,
            onPress,
            onClick,
        };
    }, [onDragStart, onDragMove, onDragEnd, onPress, onClick]);

    const [isDragging, setIsDragging] = useState(false);

    const isDraggingRef = useRef(false);
    const hasHandledPointerUpRef = useRef(false);

    const subscribePointerDown = useChessboardStore(
        (x) => x.subscribePointerDown,
    );
    const unsubscribePointerDown = useChessboardStore(
        (x) => x.unsubscribePointerDown,
    );
    const subscribePointerUp = useChessboardStore((x) => x.subscribePointerUp);
    const unsubscribePointerUp = useChessboardStore(
        (x) => x.unsubscribePointerUp,
    );

    const startDragging = useCallback((startFrom: Point) => {
        setIsDragging(true);
        callbacksRef?.current.onDragStart?.(startFrom);
        isDraggingRef.current = true;

        let animationFrameId: number | null = null;
        let lastMouseX = startFrom.x;
        let lastMouseY = startFrom.y;

        function emitDrag(): void {
            if (!isDraggingRef.current) return;

            callbacksRef?.current.onDragMove?.({
                x: lastMouseX,
                y: lastMouseY,
            });
            animationFrameId = null;
        }

        function handleMove(event: PointerEvent) {
            lastMouseX = event.clientX;
            lastMouseY = event.clientY;

            if (animationFrameId == null) {
                animationFrameId = requestAnimationFrame(emitDrag);
            }
        }

        async function stopDragging(event: PointerEvent) {
            if (hasHandledPointerUpRef.current) return;

            callbacksRef?.current.onDragEnd?.({
                x: event.clientX,
                y: event.clientY,
            });
            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);

            setIsDragging(false);
            hasHandledPointerUpRef.current = true;
            isDraggingRef.current = false;
        }

        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);
        emitDrag();
    }, []);

    useEffect(() => {
        function handler(info: InteractionInfo) {
            hasHandledPointerUpRef.current = false;

            if (shouldStartDrag(info)) startDragging(info.point);
            callbacksRef?.current.onPress?.(info);
        }

        subscribePointerDown(handler);
        return () => unsubscribePointerDown(handler);
    }, [
        subscribePointerDown,
        unsubscribePointerDown,
        shouldStartDrag,
        startDragging,
    ]);

    useEffect(() => {
        async function handler(info: InteractionInfo): Promise<void> {
            if (isDraggingRef.current || hasHandledPointerUpRef.current) return;

            callbacksRef?.current.onClick?.(info);
            hasHandledPointerUpRef.current = true;
        }

        subscribePointerUp(handler);
        return () => unsubscribePointerUp(handler);
    }, [subscribePointerUp, unsubscribePointerUp]);

    return isDragging;
}
