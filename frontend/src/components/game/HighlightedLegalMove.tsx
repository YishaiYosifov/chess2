import styles from "./HighlightedLegalMove.module.scss";
import ChessSquare from "./ChessSquare";
import { Point } from "@/models";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    return (
        <ChessSquare
            className={styles.highlightedLegalMove}
            position={position}
        />
    );
};
export default HighlightedLegalMove;
