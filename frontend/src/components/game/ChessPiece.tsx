import { memo } from "react";

import { useBoardSize, usePiece } from "@/hooks/useChess";
import styles from "./ChessPiece.module.scss";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = usePiece(id);
    const [boardWidth, boardHeight] = useBoardSize();
    if (!piece) return;

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    const [x, y] = position;

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;

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

export default memo(ChessPiece);
