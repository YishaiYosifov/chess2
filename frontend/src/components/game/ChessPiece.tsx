import { useChessStore } from "@/contexts/chessStoreContext";
import styles from "./ChessPiece.module.scss";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = useChessStore((state) => state.pieces.get(id));
    if (!piece) throw Error;

    const boardWidth = useChessStore((state) => state.boardWidth);
    const boardHeight = useChessStore((state) => state.boardHeight);
    const movePiece = useChessStore((state) => state.movePiece);

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    const [x, y] = position;

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

    return (
        <div
            data-testid="piece"
            className={styles.piece}
            onClick={() =>
                movePiece(position, [
                    Math.floor(Math.random() * 10),
                    Math.floor(Math.random() * 10),
                ])
            }
            style={{
                backgroundImage: `url("/assets/pieces/${pieceType}-${color}.png")`,
                transform: `translate(${physicalX}%, ${physicalY}%)`,
            }}
        />
    );
};

export default ChessPiece;
