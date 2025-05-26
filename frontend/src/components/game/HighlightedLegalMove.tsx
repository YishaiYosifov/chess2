import { useEventWebSocket } from "@/hooks/useEventWS";
import { useChessStore } from "@/hooks/useChess";
import { Point } from "@/types/tempModels";

import ChessSquare from "./ChessSquare";

const HighlightedLegalMove = ({ position }: { position: Point }) => {
    const { sendEventMessage } = useEventWebSocket();
    const sendMove = useChessStore((state) => state.sendMove);

    return (
        <ChessSquare
            onPointerUp={() => sendMove(sendEventMessage, position)}
            className="z-20 bg-radial-[at_20%_23%] from-black/75 to-transparent hover:border
                hover:border-white/50"
            position={position}
        />
    );
};
export default HighlightedLegalMove;
