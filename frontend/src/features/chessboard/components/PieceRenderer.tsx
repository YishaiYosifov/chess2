import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";
import HighlightedLegalMove from "./HighlightedLegalMove";
import { pointToStr } from "@/features/point/pointUtils";
import PromotionPrompt from "./PromotionPrompt";
import IntermediateSquarePrompt from "./IntermediateSquarePrompt";

const PieceRenderer = () => {
    const { pieces, highlightedLegalMoves } = useChessboardStore((x) => ({
        pieces: x.animatingPieces ?? x.pieces,
        removingPieces: x.removingPieceIds,
        highlightedLegalMoves: x.highlightedLegalMoves,
    }));

    return (
        <>
            {[...pieces].map((piece) => (
                <ChessPiece id={piece.id} key={piece.id} />
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
