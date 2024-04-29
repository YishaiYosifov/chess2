import styles from "./HighlightedLegalMove.module.scss";
import { useChessStore } from "@/hooks/useChess";
import ChessSquare from "./ChessSquare";
import { Point } from "@/models";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    const sendPieceMovement = useChessStore((state) => state.sendPieceMovement);

    return (
        <ChessSquare
            onClick={() => sendPieceMovement(position)}
            className={styles.highlightedLegalMove}
            position={position}
        />
    );
};
export default HighlightedLegalMove;
