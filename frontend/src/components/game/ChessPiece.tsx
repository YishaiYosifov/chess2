import { memo, useRef, useState, MouseEvent as ReactMouseEvent } from "react";

import { useChessStore, usePiece } from "@/hooks/useChess";
import { PieceID, type Point } from "@/models";
import styles from "./ChessPiece.module.scss";
import ChessSquare from "./ChessSquare";

export const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<HTMLDivElement>(null);
    const [draggingOffset, setDraggingOffset] = useState<Point>([0, 0]);

    const piece = usePiece(id);

    const isFixed = useChessStore((state) => state.fixed);
    const playingSide = useChessStore((state) => state.playingSide);

    const showLegalMoves = useChessStore((state) => state.showLegalMoves);

    if (!piece) return;

    const { position, pieceType, color } = piece;

    function startDragging(event: ReactMouseEvent): void {
        const canDrag = !isFixed && playingSide == color;
        if (!canDrag) return;

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
            className={styles.piece}
            ref={pieceRef}
            onPointerDown={(event) => {
                showLegalMoves(id);
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
