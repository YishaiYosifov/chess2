import { ProcessedMoveOptions } from "@/features/chessboard/lib/types";

export function createMoveOptions(
    overrides: Partial<ProcessedMoveOptions> = {},
): ProcessedMoveOptions {
    return {
        legalMoves: new Map(),
        hasForcedMoves: false,
        ...overrides,
    };
}
