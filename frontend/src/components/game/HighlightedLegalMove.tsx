import styles from "./HighlightedLegalMove.module.scss";
import { useChessStore } from "@/hooks/useChess";
import ChessSquare from "./ChessSquare";
import { Point, WSEventOut } from "@/models";
import { useEventWebSocket } from "@/hooks/useEventWS";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    const { sendEventMessage } = useEventWebSocket();
    const sendMove = useChessStore((state) => state.sendMove);

    return (
        <ChessSquare
            onPointerUp={() => sendMove(sendEventMessage, position)}
            className={styles.highlightedLegalMove}
            position={position}
        />
    );
};
export default HighlightedLegalMove;
