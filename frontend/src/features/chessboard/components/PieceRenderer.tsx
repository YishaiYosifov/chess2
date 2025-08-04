import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import ChessPiece from "./ChessPiece";
import HighlightedLegalMove from "./HighlightedLegalMove";
import { pointToStr } from "@/lib/utils/pointUtils";
import PromotionPrompt from "./PromotionPrompt";

const PieceRenderer = () => {
    const { pieces, highlightedLegalMoves } = useChessboardStore((x) => ({
        pieces: x.pieces,
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
        </>
    );
};
export default PieceRenderer;
