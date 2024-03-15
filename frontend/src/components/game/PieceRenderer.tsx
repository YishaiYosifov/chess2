import { usePieces } from "@/hooks/useChess";
import ChessPiece from "./ChessPiece";

const PieceRenderer = () => {
    const pieces = usePieces();
    return [...pieces].map(([id]) => <ChessPiece id={id} key={id} />);
};
export default PieceRenderer;
