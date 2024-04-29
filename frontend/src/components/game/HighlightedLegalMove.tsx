import styles from "./HighlightedLegalMove.module.scss";
import ChessSquare from "./ChessSquare";
import { Point } from "@/models";
import { useChessStore } from "@/hooks/useChess";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    const sendMovePieceSocket = useChessStore(
        (state) => state.sendMovePieceSocket
    );

    return (
        <ChessSquare
            onClick={() => sendMovePieceSocket(position)}
            className={styles.highlightedLegalMove}
            position={position}
        />
    );
};
export default HighlightedLegalMove;
