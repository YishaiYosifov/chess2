import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";
import HighlightedLegalMove from "./HighlightedLegalMove";
import { pointToStr } from "@/features/point/pointUtils";
import PromotionPrompt from "./PromotionPrompt";
import IntermediateSquarePrompt from "./IntermediateSquarePrompt";

const PieceRenderer = () => {
    const highlightedLegalMoves = useChessboardStore(
        (x) => x.highlightedLegalMoves,
    );
    const pieceIds = useChessboardStore((x) =>
        Array.from(x.animatingPieces?.keys() ?? x.pieces.keys()),
    );

    return (
        <>
            {[...pieceIds].map((pieceId) => (
                <ChessPiece id={pieceId} key={pieceId} />
            ))}

            {highlightedLegalMoves.map((point) => (
                <HighlightedLegalMove
                    position={point}
                    key={pointToStr(point)}
                />
            ))}

            <PromotionPrompt />
            <IntermediateSquarePrompt />
        </>
    );
};
export default PieceRenderer;
