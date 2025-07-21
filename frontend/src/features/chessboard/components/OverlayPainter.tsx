import { MouseEvent as ReactMouseEvent } from "react";

import { useChessboardStore } from "../hooks/useChessboard";
import OverlayRenderer from "./OverlayRenderer";
import { Point } from "@/types/tempModels";
import { pointEquals } from "@/lib/utils/pointUtils";

const OverlayPainter = () => {
    const commitCurrentlyDrawing = useChessboardStore(
        (x) => x.commitCurrentlyDrawing,
    );
    const setCurrentlyDrawing = useChessboardStore(
        (x) => x.setCurrentlyDrawing,
    );
    const screenPointToBoardPoint = useChessboardStore(
        (x) => x.screenPointToBoardPoint,
    );
    const clearArrows = useChessboardStore((x) => x.clearOverlays);

    function handleMouseDown(event: React.MouseEvent) {
        if (event.button !== 2) {
            clearArrows();
            return;
        }

        const startPoint = screenPointToBoardPoint({
            x: event.clientX,
            y: event.clientY,
        });
        if (!startPoint) return;

        setCurrentlyDrawing(startPoint, startPoint);

        let lastSquare: Point = startPoint;
        function handleMove(event: MouseEvent | ReactMouseEvent) {
            const movePoint = screenPointToBoardPoint({
                x: event.clientX,
                y: event.clientY,
            });
            if (!movePoint || !startPoint || pointEquals(movePoint, lastSquare))
                return;

            lastSquare = movePoint;
            setCurrentlyDrawing(startPoint, movePoint);
        }

        async function stopDragging(): Promise<void> {
            commitCurrentlyDrawing();

            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);
        }

        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);
    }

    return (
        <div className="absolute inset-0 z-20" onMouseDown={handleMouseDown}>
            <OverlayRenderer />
        </div>
    );
};
export default OverlayPainter;
