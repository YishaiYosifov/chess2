import { memo, useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { Point } from "@/features/point/types";
import { PieceID } from "../lib/types";

import ChessSquare from "./ChessSquare";
import useBoardInteraction from "../hooks/useBoardInteraction";
import getPieceImage from "../lib/pieceImage";
import { ChessSquareRef } from "./CoordSquare";

const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<ChessSquareRef>(null);
    const {
        piece,
        isSelected,
        isAnimating,
        isRemoving,
        canDrag,
        screenPointToPiece,
        selectPiece,
        handleMousePieceDrop,
    } = useChessboardStore((x) => ({
        piece: x.animatingPieceMap?.get(id) ?? x.pieceMap.get(id),
        isSelected: x.selectedPieceId === id,
        isAnimating: x.animatingPieces.has(id),
        isRemoving: x.removingPieces.has(id),
        canDrag: x.canDrag,
        screenPointToPiece: x.screenPointToPiece,
        selectPiece: x.selectPiece,
        handleMousePieceDrop: x.handleMousePieceDrop,
    }));

    const offsetRef = useRef<Point | null>(null);
    const moveOccurredOnPressRef = useRef<boolean>(false);
    const isDragging = useBoardInteraction({
        shouldStartDrag(info) {
            if (moveOccurredOnPressRef.current) {
                moveOccurredOnPressRef.current = false;
                return false;
            }

            if (!canDrag || info.button !== 0) return false;

            const piece = screenPointToPiece(info.point);
            return piece === id;
        },

        onDragStart() {
            selectPiece(id);
            const rect = pieceRef.current?.getBoundingClientRect();
            if (!rect) return;

            // make sure the piece snaps to the cursor
            const offsetX = rect.left + rect.width / 2;
            const offsetY = rect.top + rect.height / 2;
            offsetRef.current = { x: offsetX, y: offsetY };
        },
        onDragMove(point) {
            if (!offsetRef.current) return;

            const x = point.x - offsetRef.current.x;
            const y = point.y - offsetRef.current.y;
            pieceRef.current?.updateDraggingOffset({ x, y });
        },

        async onDragEnd(point) {
            await handleMousePieceDrop({
                mousePoint: point,
                isDrag: true,
            });
            pieceRef.current?.updateDraggingOffset({ x: 0, y: 0 });
        },
        async onPress(info) {
            if (!isSelected || info.button != 0) return;

            const didMove = await handleMousePieceDrop({
                mousePoint: info.point,
                isDrag: false,
            });
            if (didMove) moveOccurredOnPressRef.current = true;
            pieceRef.current?.updateDraggingOffset({ x: 0, y: 0 });
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
                isRemoving && "opacity-50",
            )}
            ref={pieceRef}
            style={{
                backgroundImage: `url("${getPieceImage(piece.type, piece.color)}")`,
            }}
        />
    );
};

export default memo(ChessPiece);
