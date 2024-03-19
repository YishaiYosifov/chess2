import { memo } from "react";

import {
    useBoardSize,
    useChessStore,
    usePiece,
    useViewingFrom,
} from "@/hooks/useChess";
import styles from "./ChessPiece.module.scss";
import { Color } from "./chess.types";

export const ChessPiece = ({ id }: { id: string }) => {
    const piece = usePiece(id);
    const [boardWidth, boardHeight] = useBoardSize();
    const viewingFrom = useViewingFrom();

    if (!piece) return;

    const { position, pieceType, color } = piece;

    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the white prespective
    if (viewingFrom == Color.White)
        [x, y] = [boardWidth - x - 1, boardHeight - y - 1];

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
