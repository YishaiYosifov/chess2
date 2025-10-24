import { useChessboardStore } from "../hooks/useChessboard";
import ChessSquare from "./ChessSquare";

const IntermediateSquarePrompt = () => {
    const { nextIntermediates, resolveNextIntermediate } = useChessboardStore(
        (x) => ({
            nextIntermediates: x.nextIntermediates,
            resolveNextIntermediate: x.resolveNextIntermediate,
        }),
    );
    if (nextIntermediates.length === 0) return null;

    return (
        <div
            data-testid="intermediateSquarePromptOverlay"
            className="absolute inset-0 z-50 flex cursor-auto bg-black/50"
            onMouseDown={(e) => {
                resolveNextIntermediate?.(null);
                e.stopPropagation();
            }}
        >
            {nextIntermediates.map((point, i) => (
                <ChessSquare
                    data-testid="intermediateSquare"
                    position={point}
                    key={i}
                    onMouseDown={(e) => e.stopPropagation()}
                    onClick={(e) => {
                        e.stopPropagation();
                        resolveNextIntermediate?.(point);
                    }}
                    className="z-30 animate-[fadeIn_0.15s_ease-out] cursor-pointer rounded-sm border-4
                        border-yellow-400 bg-[length:100%_100%] bg-center bg-no-repeat transition-colors
                        duration-100 ease-out hover:bg-[rgba(255,215,0,0.2)]"
                />
            ))}
        </div>
    );
};
export default IntermediateSquarePrompt;
