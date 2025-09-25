import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";
import HighlightedLegalMove from "./HighlightedLegalMove";
import { pointToStr } from "@/features/point/pointUtils";
import PromotionPrompt from "./PromotionPrompt";
import IntermediateSquarePrompt from "./IntermediateSquarePrompt";

const PieceRenderer = () => {
    const { pieces, highlightedLegalMoves } = useChessboardStore((x) => ({
        pieces: x.animatingPieceMap ?? x.pieceMap,
        removingPieces: x.removingPieces,
        highlightedLegalMoves: x.highlightedLegalMoves,
    }));

    return (
        <>
            {[...pieces].map(([id]) => (
                <ChessPiece id={id} key={id} />
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
