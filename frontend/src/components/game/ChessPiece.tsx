import { memo, useRef, useState, MouseEvent as ReactMouseEvent } from "react";

import { useChessStore, usePiece } from "@/hooks/useChess";
import { PieceID } from "@/types/tempModels";

import ChessSquare, { ChessSquareRef } from "./ChessSquare";
import clsx from "clsx";

export const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<ChessSquareRef>(null);

    const piece = usePiece(id);
    const sideToMove = useChessStore((state) => state.sideToMove);
    const playingAs = useChessStore((state) => state.playingAs);
    const showLegalMoves = useChessStore((state) => state.showLegalMoves);
    const handlePieceDrop = useChessStore((state) => state.handlePieceDrop);
    const isAnimating = useChessStore((state) => state.animatingPieces.has(id));

    const [isDragging, setIsDragging] = useState(false);

    if (!piece) return;

    function startDragging(event: ReactMouseEvent): void {
        if (!pieceRef.current) return;

        setIsDragging(true);

        // calculate the dragging offset
        // snap the center of the piece to the mouse when dragging start
        const rect = pieceRef.current.getBoundingClientRect();
        if (!rect) return;

        const offsetX = rect.left + rect.width / 2;
        const offsetY = rect.top + rect.height / 2;

        let didStopDragging = false;
        let animationFrameId: number | null = null;
        let lastMouseX = 0;
        let lastMouseY = 0;

        function updateDraggingOffset(): void {
            if (didStopDragging) return;

            const x = lastMouseX - offsetX;
            const y = lastMouseY - offsetY;
            pieceRef.current?.updateDraggingOffset(x, y);
            animationFrameId = null;
        }

        // calculate the new offset when the mouse moves
        function handleMove(event: MouseEvent | ReactMouseEvent) {
            lastMouseX = event.clientX;
            lastMouseY = event.clientY;

            if (animationFrameId == null) {
                animationFrameId = requestAnimationFrame(() =>
                    updateDraggingOffset(),
                );
            }
        }

        // reset the event listeners and the dragging offset
        async function stopDragging(): Promise<void> {
            setIsDragging(false);
            didStopDragging = true;
            await handlePieceDrop(lastMouseX, lastMouseY);
            pieceRef.current?.updateDraggingOffset(0, 0);

            window.removeEventListener("pointermove", handleMove);
            window.removeEventListener("pointerup", stopDragging);
        }

        // add event listeners for mouse movement and release
        window.addEventListener("pointermove", handleMove);
        window.addEventListener("pointerup", stopDragging);

        handleMove(event);
    }

    return (
        <ChessSquare
            data-testid="piece"
            position={piece.position}
            className={clsx(
                "z-10 touch-none bg-size-[length:100%] bg-no-repeat select-none",
                isAnimating && "transition-transform duration-100 ease-out",
                isDragging && "pointer-events-none z-30",
            )}
            ref={pieceRef}
            onPointerDown={(event) => {
                const canDrag =
                    playingAs == sideToMove && piece.color == sideToMove;
                if (!canDrag) return;

                showLegalMoves(id);
                startDragging(event);
            }}
            style={{
                backgroundImage: `url("/assets/pieces/${piece.type}${piece.color}.png")`,
            }}
        />
    );
};

export default memo(ChessPiece);
