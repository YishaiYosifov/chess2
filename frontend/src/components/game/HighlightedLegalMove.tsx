import styles from "./HighlightedLegalMove.module.scss";
import { Point } from "./chess.types";
import ChessSquare from "./ChessSquare";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    return (
        <ChessSquare
            className={styles.highlightedLegalMove}
            position={position}
        />
    );
};
export default HighlightedLegalMove;
