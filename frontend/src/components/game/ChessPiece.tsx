import { useChessStore } from "@/contexts/chessStoreContext";
import styles from "./ChessPiece.module.scss";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = useChessStore((state) => state.pieces.get(id));
    if (!piece) throw Error;

    const boardWidth = useChessStore((state) => state.boardWidth);
    const boardHeight = useChessStore((state) => state.boardHeight);

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    const [x, y] = position;
    const [physicalX, physicalY] = [
        x * boardWidth * boardHeight,
        y * boardSize,
    ];

    return (
        <div
            data-testid="piece"
            className={styles.piece}
            style={{
                backgroundImage: `url("/assets/pieces/${pieceType}-${color}.png")`,
                transform: `translate(${physicalX}%, ${physicalY}%)`,
            }}
        />
    );
};

export default ChessPiece;
