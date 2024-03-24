import {
    memo,
    useRef,
    useState,
    MouseEvent as ReactMouseEvent,
    TouchEvent as ReactTouchEvent,
} from "react";

import { useBoardSize, useChessStore, usePiece } from "@/hooks/useChess";
import styles from "./ChessPiece.module.scss";
import { Color, Point } from "./chess.types";

export const ChessPiece = ({ id }: { id: string }) => {
    const pieceRef = useRef<HTMLDivElement>(null);
    const [draggingOffset, setDraggingOffset] = useState<Point>([0, 0]);

    const piece = usePiece(id);
    const [boardWidth, boardHeight] = useBoardSize();

    const isFixed = useChessStore((state) => state.fixed);
    const playingSide = useChessStore((state) => state.playingSide);
    const viewingFrom = useChessStore((state) => state.viewingFrom);

    if (!piece) return;

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the black prespective
    if (viewingFrom == Color.Black)
        [x, y] = [boardWidth - x - 1, boardHeight - y - 1];

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

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
        <div
            data-testid="piece"
            className={styles.piece}
            ref={pieceRef}
            onPointerDown={startDragging}
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
