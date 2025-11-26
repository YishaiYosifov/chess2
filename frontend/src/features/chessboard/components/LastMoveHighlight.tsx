import { useChessboardStore } from "../hooks/useChessboard";
import ChessSquare from "./ChessSquare";

const LastMoveHighlight = () => {
    const fromPosition = useChessboardStore((x) => x.lastMove?.from);
    const toPosition = useChessboardStore((x) => x.lastMove?.to);

    return (
        <>
            {fromPosition && (
                <ChessSquare
                    position={fromPosition}
                    className="bg-accent/60"
                    data-testid="highlightedLastMoveFrom"
                />
            )}
            {toPosition && (
                <ChessSquare
                    position={toPosition}
                    className="bg-accent/60"
                    data-testid="highlightedLastMoveTo"
                />
            )}
        </>
    );
};
export default LastMoveHighlight;
