import { useEventWebSocket } from "@/hooks/useEventWS";
import { useChessStore } from "@/hooks/useChess";
import { Point } from "@/lib/apiClient/models";

import ChessSquare from "./ChessSquare";

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
