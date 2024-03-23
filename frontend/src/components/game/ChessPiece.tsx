import { memo, useEffect, useRef, useState } from "react";

import { useBoardSize, usePiece, useViewingFrom } from "@/hooks/useChess";
import styles from "./ChessPiece.module.scss";
import { Color, Point } from "./chess.types";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = usePiece(id);
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useViewingFrom();
    const pieceRef = useRef<HTMLDivElement>(null);

    const [dragPosition, setDragPosition] = useState<Point>([0, 0]);
    const [startingPosition, setStartingPosition] = useState<Point>([0, 0]);
    const [isDragging, setIsDragging] = useState<boolean>(false);

    useEffect(() => {
        if (!isDragging) return;

        function disableDragging() {
            document.removeEventListener("mousemove", onMove);
            setDragPosition([0, 0]);
            setIsDragging(false);
        }

        function onMove(event: MouseEvent) {
            event.preventDefault();
            //if (!isDragging) return;

            const x = event.clientX;
            const y = event.clientY;

            setDragPosition([x - startingPosition[0], y - startingPosition[1]]);
        }

        document.addEventListener("mousemove", onMove);
        document.addEventListener("mouseup", disableDragging);

        return disableDragging;
    }, [isDragging, startingPosition]);

    if (!piece) return;
    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the white prespective
    if (viewingFrom == Color.White)
        [x, y] = [boardWidth - x - 1, boardHeight - y - 1];

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

    return (
        <div
            data-testid="piece"
            className={styles.piece}
            ref={pieceRef}
            draggable={false}
            onMouseDown={(event) => {
                setStartingPosition([event.clientX, event.clientY]);
                setIsDragging(true);
            }}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceType}-${color}.png")`,
                transform: `translate(${physicalX + dragPosition[0]}%, ${
                    physicalY + dragPosition[1]
                }%)`,
            }}
        />
    );
};

export default memo(ChessPiece);
