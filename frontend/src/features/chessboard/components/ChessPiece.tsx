import { memo, useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { Point } from "@/features/point/types";
import { PieceID } from "../lib/types";

import ChessSquare from "./ChessSquare";
import useBoardInteraction from "../hooks/useBoardInteraction";
import getPieceImage from "../lib/pieceImage";
import { ChessSquareRef } from "./CoordSquare";
import DoubleClickIndicator, { DoubleClickRef } from "./DoubleClickIndicator";

const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<ChessSquareRef>(null);
    const {
        pieceType,
        pieceColor,
        isSelected,
        isAnimating,
        isRemoving,
        canDrag,
        screenPointToPiece,
        selectPiece,
        unselectPiece,
        handleMousePieceDrop,
    } = useChessboardStore((x) => {
        const piece = x.animatingPieces?.getById(id) ?? x.pieces.getById(id);
        return {
            pieceType: piece?.type,
            pieceColor: piece?.color,
            isSelected: x.selectedPieceId === id,
            isAnimating: x.animatingPieceIds.has(id),
            isRemoving: x.removingPieceIds.has(id),
            canDrag: x.canDrag,
            screenPointToPiece: x.screenPointToPiece,
            selectPiece: x.selectPiece,
            unselectPiece: x.unselectPiece,
            handleMousePieceDrop: x.handleMousePieceDrop,
        };
    });
    const piecePosition = useChessboardStore(
        (x) =>
            (x.animatingPieces?.getById(id) ?? x.pieces.getById(id))?.position,
    );

    const doubleClickRef = useRef<DoubleClickRef>(null);

    const offsetRef = useRef<Point | null>(null);
    const moveOccurredOnPressRef = useRef(false);
    const lastClickTimeRef = useRef(0);
    const wasJustSelectedRef = useRef(false);

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
            const rect = pieceRef.current?.getBoundingClientRect();
            if (!rect) return;

            // make sure the piece snaps to the cursor
            const offsetX = rect.left + rect.width / 2;
            const offsetY = rect.top + rect.height / 2;
            offsetRef.current = { x: offsetX, y: offsetY };

            wasJustSelectedRef.current = selectPiece(id);
        },
        onDragMove(point) {
            if (!offsetRef.current) return;

            const x = point.x - offsetRef.current.x;
            const y = point.y - offsetRef.current.y;
            pieceRef.current?.updateDraggingOffset({ x, y });
        },

        async onDragEnd(point) {
            const now = Date.now();
            const isDoubleClick = now - lastClickTimeRef.current < 500;
            lastClickTimeRef.current = Date.now();

            const { needsDoubleClick } = await handleMousePieceDrop({
                mousePoint: point,
                isDrag: true,
                isDoubleClick,
            });
            if (needsDoubleClick) doubleClickRef.current?.trigger();

            pieceRef.current?.updateDraggingOffset({ x: 0, y: 0 });
            if (!wasJustSelectedRef.current) unselectPiece();
        },
        async onPress(info) {
            if (!isSelected || info.button != 0) return;

            const now = Date.now();
            const isDoubleClick = now - lastClickTimeRef.current < 500;

            const { success } = await handleMousePieceDrop({
                mousePoint: info.point,
                isDrag: false,
                isDoubleClick,
            });
            if (success) moveOccurredOnPressRef.current = true;
            pieceRef.current?.updateDraggingOffset({ x: 0, y: 0 });
        },
    });

    if (
        piecePosition === undefined ||
        pieceType === undefined ||
        pieceColor === undefined
    )
        return null;
    return (
        <>
            <ChessSquare
                data-testid="piece"
                position={piecePosition}
                className={clsx(
                    `pointer-events-none z-10 touch-none bg-size-[length:100%] bg-no-repeat
                    transition-colors select-none`,
                    isAnimating && "transition-transform duration-100 ease-out",
                    isDragging && "z-30",
                    isRemoving && "opacity-50",
                )}
                ref={pieceRef}
                style={{
                    backgroundImage: `url("${getPieceImage(pieceType, pieceColor)}")`,
                }}
            >
                <DoubleClickIndicator ref={doubleClickRef} />
            </ChessSquare>
            {isSelected && (
                <ChessSquare
                    data-testid="pieceSquareHighlight"
                    position={piecePosition}
                    className="bg-secondary/50 pointer-events-none z-5 touch-none"
                />
            )}
        </>
    );
};

export default memo(ChessPiece);
