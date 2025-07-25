import { ProcessedMoveOptions } from "@/types/tempModels";

export function createMoveOptions(
    overrides: Partial<ProcessedMoveOptions> = {},
): ProcessedMoveOptions {
    return {
        legalMoves: new Map(),
        hasForcedMoves: false,
        ...overrides,
    };
}
