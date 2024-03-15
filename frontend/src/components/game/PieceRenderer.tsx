import { useChessStore } from "@/contexts/chessStoreContext";
import ChessPiece from "./ChessPiece";

const PieceRenderer = () => {
    const pieces = useChessStore((state) => state.pieces);
    return [...pieces].map(([id]) => <ChessPiece id={id} key={id} />);
};
export default PieceRenderer;
