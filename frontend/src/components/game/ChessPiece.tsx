import { Color } from "@/client";
import styles from "./ChessPiece.module.scss";
import { useChessStore } from "@/contexts/chessStoreContext";

export const ChessPiece = ({
    boardWidth,
    boardHeight,
    id,
    fixed = false,
    viewingFrom: side = Color.White,
}: {
    boardWidth: number;
    boardHeight: number;
    id: string;
    fixed?: boolean;
    viewingFrom?: Color;
}) => {
    const piece = useChessStore((state) => state.pieces.get(id));
    if (!piece) throw Error;

    const { position, pieceType, color } = piece;
    if (side == Color.Black) position[1] = boardHeight - position[1] - 1;

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
