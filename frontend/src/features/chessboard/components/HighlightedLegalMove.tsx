"use client";

import { LogicalPoint } from "@/types/tempModels";

import ChessSquare from "./ChessSquare";

const HighlightedLegalMove = ({ position }: { position: LogicalPoint }) => {
    return (
        <ChessSquare
            data-testid="highlightedLegalMove"
            className="z-20 animate-[fadeIn_0.15s_ease-out]
                bg-[radial-gradient(rgba(0,0,0,0.25)_20%,_rgba(0,0,0,0)_23%)]
                bg-[length:100%_100%] bg-center bg-no-repeat transition-all duration-100
                ease-out hover:border-5 hover:border-white/50 hover:bg-[rgba(105,105,105,0.2)]"
            position={position}
        />
    );
};
export default HighlightedLegalMove;
