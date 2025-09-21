import clsx from "clsx";
import { useChessboardStore } from "../hooks/useChessboard";
import ChessSquare from "./ChessSquare";
import { PieceType } from "@/lib/apiClient";
import { logicalPoint } from "@/features/point/pointUtils";
import getPieceImage from "../lib/pieceImage";
import { PromotionRequest } from "../stores/promotionSlice";

const PromotionPrompt = () => {
    const { pendingPromotion, resolvePromotion } = useChessboardStore((x) => ({
        pendingPromotion: x.pendingPromotion,
        resolvePromotion: x.resolvePromotion,
    }));
    if (!pendingPromotion) return;

    return (
        <div
            data-testid="promotionPromptOverlay"
            className="absolute inset-0 z-50 flex cursor-auto bg-black/60"
            onMouseDown={() => resolvePromotion?.(null)}
        >
            {pendingPromotion.pieces.map((piece, i) => (
                <PromotionPiece
                    key={i}
                    index={i}
                    pendingPromotion={pendingPromotion}
                    piece={piece}
                />
            ))}
        </div>
    );
};
export default PromotionPrompt;

const PromotionPiece = ({
    index,
    pendingPromotion,
    piece,
}: {
    index: number;
    pendingPromotion: PromotionRequest;
    piece: PieceType | null;
}) => {
    const { resolvePromotion, boardDimensions } = useChessboardStore((x) => ({
        resolvePromotion: x.resolvePromotion,
        boardDimensions: x.boardDimensions,
    }));

    if (!piece) return;

    function choosePiece(event: React.MouseEvent, piece: PieceType) {
        event.stopPropagation();
        resolvePromotion?.(piece);
    }

    const isCloserToBottom =
        boardDimensions.height - pendingPromotion.at.y >
        boardDimensions.height / 2;
    const delta = isCloserToBottom ? index : -index;

    const position = logicalPoint({
        x: pendingPromotion.at.x,
        y: pendingPromotion.at.y + delta,
    });

    const isFirst = index === 0;
    const isLast = index === pendingPromotion.pieces.length - 1;
    return (
        <ChessSquare
            data-testid="promotionPiece"
            position={position}
            className={clsx(
                `border-secondary hover:bg-secondary cursor-pointer rounded-md border-3
                bg-[length:90%_90%] bg-center bg-no-repeat transition-all duration-200
                hover:rounded-none hover:bg-[length:110%_110%]`,
                isFirst || "border-t-2",
                isLast || "border-b-2",
            )}
            style={{
                backgroundImage: `url("${getPieceImage(piece, pendingPromotion.piece.color)}")`,
            }}
            onMouseDown={(e) => e.stopPropagation()}
            onClick={(e) => choosePiece(e, piece)}
        />
    );
};
