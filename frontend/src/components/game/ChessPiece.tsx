import { memo, useRef, useState, MouseEvent as ReactMouseEvent } from "react";

import { useChessStore, usePiece } from "@/hooks/useChess";
import { PieceID, type Point } from "@/types/tempModels";

import ChessSquare from "./ChessSquare";
import clsx from "clsx";

export const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<HTMLDivElement>(null);
    const [draggingOffset, setDraggingOffset] = useState<Point>([0, 0]);

    const piece = usePiece(id);
    const sideToMove = useChessStore((state) => state.sideToMove);
    const playingAs = useChessStore((state) => state.playingAs);
    const showLegalMoves = useChessStore((state) => state.showLegalMoves);

    const [isDragging, setIsDragging] = useState(false);

    if (!piece) return;

    const { position, pieceType, color } = piece;

    function startDragging(event: ReactMouseEvent): void {
        setIsDragging(true);

        // calculate the dragging offset
        // snap the center of the piece to the mouse when dragging start
        const rect = pieceRef.current!.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;

        const offsetX = event.clientX + (centerX - event.clientX);
        const offsetY = event.clientY + (centerY - event.clientY);

        function updateDraggingOffset(mouseX: number, mouseY: number): void {
            const x = mouseX - offsetX;
            const y = mouseY - offsetY;
            setDraggingOffset([x, y]);
        }

        // calculate the new offset when the mouse moves
        const handleMove = (event: MouseEvent) =>
            updateDraggingOffset(event.clientX, event.clientY);

        // reset the event listeners and the dragging offset
        function stopDragging(): void {
            setIsDragging(false);
            setDraggingOffset([0, 0]);

            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);
        }

        // add event listeners for mouse movement and release
        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);
        updateDraggingOffset(event.clientX, event.clientY);
    }

    return (
        <ChessSquare
            data-testid="piece"
            position={position}
            className={clsx(
                `z-10 touch-none bg-size-[length:100%] bg-no-repeat transition-transform
                duration-100 ease-out select-none`,
                isDragging && "pointer-events-none z-20",
            )}
            ref={pieceRef}
            onPointerDown={(event) => {
                const canDrag = playingAs == sideToMove && color == sideToMove;
                if (!canDrag) return;

                showLegalMoves(position);
                startDragging(event);
            }}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceType}-${color}.png")`,
                left: draggingOffset[0],
                top: draggingOffset[1],
            }}
        />
    );
};

export default memo(ChessPiece);
