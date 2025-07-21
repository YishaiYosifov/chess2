import { useCallback, useEffect, useState } from "react";
import { InteractionInfo } from "../stores/interactionSlice";
import { Point } from "@/types/tempModels";
import { useChessboardStore } from "./useChessboard";

export default function useBoardInteraction({
    shouldStartDrag,

    onDragStart,
    onDragMove,
    onDragEnd,

    onInteractionStart,
    onInteractionEnd,
}: {
    shouldStartDrag: (info: InteractionInfo) => boolean;

    onDragStart?: (info: InteractionInfo) => void;
    onDragMove?: (point: Point) => void;
    onDragEnd?: (info: InteractionInfo) => void;

    onInteractionStart?: (info: InteractionInfo) => void;
    onInteractionEnd?: (info: InteractionInfo) => void;
}): boolean {
    const [isDragging, setIsDragging] = useState(false);

    const listenToPointerDown = useChessboardStore(
        (x) => x.listenToPointerDown,
    );
    const disregardPointerDown = useChessboardStore(
        (x) => x.disregardPointerDown,
    );
    const listenToPointerUp = useChessboardStore((x) => x.listenToPointerUp);
    const disregardPointerUp = useChessboardStore((x) => x.disregardPointerUp);

    const startDragging = useCallback(
        (info: InteractionInfo) => {
            onDragStart?.(info);

            setIsDragging(true);
            let didStopDragging = false;
            let animationFrameId: number | null = null;
            let lastMouseX = info.point.x;
            let lastMouseY = info.point.y;

            function emitDrag(): void {
                if (didStopDragging) return;

                onDragMove?.({ x: lastMouseX, y: lastMouseY });
                animationFrameId = null;
            }

            function handleMove(event: PointerEvent) {
                lastMouseX = event.clientX;
                lastMouseY = event.clientY;

                if (animationFrameId == null) {
                    animationFrameId = requestAnimationFrame(emitDrag);
                }
            }

            async function stopDragging() {
                didStopDragging = true;
                window.removeEventListener("pointermove", handleMove);
                window.removeEventListener("pointerup", stopDragging);
            }

            window.addEventListener("pointermove", handleMove);
            window.addEventListener("pointerup", stopDragging);
            emitDrag();
        },
        [onDragStart, onDragMove],
    );

    useEffect(() => {
        function handler(info: InteractionInfo) {
            if (shouldStartDrag(info)) startDragging(info);
            onInteractionStart?.(info);
        }

        listenToPointerDown(handler);
        return () => disregardPointerDown(handler);
    }, [
        listenToPointerDown,
        disregardPointerDown,
        shouldStartDrag,
        startDragging,
        onInteractionStart,
    ]);

    useEffect(() => {
        async function handler(info: InteractionInfo): Promise<void> {
            onInteractionEnd?.(info);
            if (isDragging) {
                onDragEnd?.(info);
                setIsDragging(false);
            }
        }

        listenToPointerUp(handler);
        return () => disregardPointerUp(handler);
    }, [
        listenToPointerUp,
        disregardPointerUp,
        onInteractionEnd,
        onDragEnd,
        isDragging,
    ]);

    return isDragging;
}
