import { memo, useRef, useState, MouseEvent as ReactMouseEvent } from "react";

import { useBoardSize, usePiece, useViewingFrom } from "@/hooks/useChess";
import styles from "./ChessPiece.module.scss";
import { Color, Point } from "./chess.types";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = usePiece(id);
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useViewingFrom();
    const pieceRef = useRef<HTMLDivElement>(null);

    const [draggingOffset, setDraggingOffset] = useState<Point>([0, 0]);

    if (!piece) return;

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the white prespective
    if (viewingFrom == Color.White)
        [x, y] = [boardWidth - x - 1, boardHeight - y - 1];

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

    /**
     * Start piece dragging
     */
    function startDragging(event: ReactMouseEvent): void {
        // calculate the initial position relative to the mouse
        const startX = event.clientX;
        const startY = event.clientY;

        // calculate the new offset when the mouse moves
        function handleMouseMove(event: MouseEvent): void {
            const x = event.clientX - startX;
            const y = event.clientY - startY;
            setDraggingOffset([x, y]);
        }

        // reset the event listeners and the dragging offset
        function stopDragging() {
            setDraggingOffset([0, 0]);
            window.removeEventListener("mousemove", handleMouseMove);
            window.removeEventListener("mouseup", stopDragging);
        }

        // add event listeners for mouse movement and release
        window.addEventListener("mousemove", handleMouseMove);
        window.addEventListener("mouseup", stopDragging);
    }

    return (
        <div
            data-testid="piece"
            className={styles.piece}
            ref={pieceRef}
            onMouseDown={startDragging}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceType}-${color}.png")`,
                transform: `translate(${physicalX}%, ${physicalY}%)`,
                left: draggingOffset[0],
                top: draggingOffset[1],
            }}
        />
    );
};

export default memo(ChessPiece);
