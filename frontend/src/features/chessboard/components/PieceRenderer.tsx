import {
    useHighlightedLegalMoves,
    usePieces,
} from "@/features/chessboard/hooks/useChess";
import ChessPiece from "./ChessPiece";
import HighlightedLegalMove from "./HighlightedLegalMove";
import { pointToStr } from "@/lib/utils/pointUtils";

const PieceRenderer = () => {
    const pieces = usePieces();
    const highlightedLegalMoves = useHighlightedLegalMoves();

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
        </>
    );
};
export default PieceRenderer;
