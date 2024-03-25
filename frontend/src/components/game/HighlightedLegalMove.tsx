import { useBoardSize, useChessStore } from "@/hooks/useChess";
import { position2Offset } from "@/lib/utils/chessUtils";
import styles from "./HighlightedLegalMove.module.scss";
import { Point } from "./chess.types";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    const viewingFrom = useChessStore((state) => state.viewingFrom);
    const [boardWidth, boardHeight] = useBoardSize();

    const [physicalX, physicalY] = position2Offset(
        position,
        viewingFrom,
        boardWidth,
        boardHeight
    );
    return (
        <div
            className={styles.highlightedLegalMove}
            style={{
                transform: `translate(${physicalX}%, ${physicalY}%)`,
            }}
        ></div>
    );
};
export default HighlightedLegalMove;
