import { memo, useRef } from "react";
import clsx from "clsx";

import {
    useChessboardStore,
    usePiece,
} from "@/features/chessboard/hooks/useChessboard";
import { PieceID, Point } from "@/types/tempModels";

import ChessSquare, { ChessSquareRef } from "./ChessSquare";
import useBoardInteraction from "../hooks/useBoardInteraction";

export const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<ChessSquareRef>(null);
    const piece = usePiece(id);

    const isSelected = useChessboardStore(
        (state) => state.selectedPieceId === id,
    );
    const isAnimating = useChessboardStore((state) =>
        state.animatingPieces.has(id),
    );

    const screenPointToPiece = useChessboardStore((x) => x.screenPointToPiece);
    const selectPiece = useChessboardStore((x) => x.selectPiece);
    const moveSelectedPieceToMouse = useChessboardStore(
        (x) => x.handleMousePieceDrop,
    );

    const offset = useRef<Point | null>(null);
    const isDragging = useBoardInteraction({
        shouldStartDrag(info) {
            if (info.button !== 0) return false;

            const piece = screenPointToPiece(info.point);
            if (piece !== id) return false;

            return true;
        },

        onDragStart() {
            selectPiece(id);
            const rect = pieceRef.current?.getBoundingClientRect();
            if (!rect) return;

            // make sure the piece snaps to the cursor
            const offsetX = rect.left + rect.width / 2;
            const offsetY = rect.top + rect.height / 2;
            offset.current = { x: offsetX, y: offsetY };
        },
        onDragMove(point) {
            if (!offset.current) return;

            const x = point.x - offset.current.x;
            const y = point.y - offset.current.y;
            pieceRef.current?.updateDraggingOffset(x, y);
        },
        async onDragEnd(point) {
            const didMove = await moveSelectedPieceToMouse({
                mousePoint: point,
                isDrag: true,
            });
            if (!didMove) pieceRef.current?.updateDraggingOffset(0, 0);
        },
        async onClick(info) {
            if (!isSelected) return;

            const didMove = await moveSelectedPieceToMouse({
                mousePoint: info.point,
                isDrag: false,
            });
            if (!didMove) pieceRef.current?.updateDraggingOffset(0, 0);
        },
    });

    if (!piece) return;

    return (
        <ChessSquare
            data-testid="piece"
            position={piece.position}
            className={clsx(
                `pointer-events-none z-10 touch-none bg-size-[length:100%] bg-no-repeat
                select-none`,
                isAnimating && "transition-transform duration-100 ease-out",
                isDragging && "z-30",
            )}
            ref={pieceRef}
            style={{
                backgroundImage: `url("/assets/pieces/${piece.type}${piece.color}.png")`,
            }}
        />
    );
};

export default memo(ChessPiece);
