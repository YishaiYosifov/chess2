import { memo, useRef } from "react";
import clsx from "clsx";

import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { Point } from "@/features/point/types";
import { PieceID } from "../lib/types";

import ChessSquare, { ChessSquareRef } from "./ChessSquare";
import useBoardInteraction from "../hooks/useBoardInteraction";
import getPieceImage from "../lib/pieceImage";

export const ChessPiece = ({ id }: { id: PieceID }) => {
    const pieceRef = useRef<ChessSquareRef>(null);
    const piece = useChessboardStore((state) => state.pieces.get(id));

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

    const offsetRef = useRef<Point | null>(null);
    const moveOccurredOnPressRef = useRef<boolean>(false);
    const isDragging = useBoardInteraction({
        shouldStartDrag(info) {
            if (moveOccurredOnPressRef.current) {
                moveOccurredOnPressRef.current = false;
                return false;
            }

            if (info.button !== 0) return false;

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
            await moveSelectedPieceToMouse({
                mousePoint: point,
                isDrag: true,
            });
            pieceRef.current?.updateDraggingOffset({ x: 0, y: 0 });
        },
        async onPress(info) {
            if (!isSelected) return;

            const didMove = await moveSelectedPieceToMouse({
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
            )}
            ref={pieceRef}
            style={{
                backgroundImage: `url("${getPieceImage(piece.type, piece.color)}")`,
            }}
        />
    );
};

export default memo(ChessPiece);
