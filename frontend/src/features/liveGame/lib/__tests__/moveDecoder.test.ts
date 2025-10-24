import brotliCompress from "brotli/compress";

import { GameColor, MovePath, PieceType } from "@/lib/apiClient";
import { decodeEncodedMovesIntoMap, decodePathIntoMap } from "../moveDecoder";
import { Move } from "@/features/chessboard/lib/types";
import { logicalPoint } from "@/features/point/pointUtils";

vi.mock("brotli/compress");

const emptyMove = {
    triggers: [],
    captures: [],
    intermediates: [],
    sideEffects: [],
    pieceSpawns: [],
    promotesTo: null,
};

describe("decodePathIntoMap", () => {
    it("should decode single path into correct LegalMoveMap entry", () => {
        const paths: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                moveKey: "2",
                triggerIdxs: [3],
                capturedIdxs: [4],
                intermediateIdxs: [5],
                sideEffects: [{ fromIdx: 6, toIdx: 7 }],
                pieceSpawns: [
                    {
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        posIdx: 8,
                    },
                ],
                promotesTo: PieceType.BISHOP,
            },
        ];

        const result = decodePathIntoMap(paths, 10);

        expect(result.size).toBe(1);
        const moves = result.get("0,0");
        expect(moves).toBeDefined();
        expect(moves).toHaveLength(1);

        const move = moves![0];
        expect(move).toEqual<Move>({
            from: logicalPoint({ x: 0, y: 0 }),
            to: logicalPoint({ x: 1, y: 0 }),
            moveKey: "2",
            triggers: [logicalPoint({ x: 3, y: 0 })],
            captures: [logicalPoint({ x: 4, y: 0 })],
            intermediates: [logicalPoint({ x: 5, y: 0 })],
            sideEffects: [
                {
                    from: logicalPoint({ x: 6, y: 0 }),
                    to: logicalPoint({ x: 7, y: 0 }),
                },
            ],
            pieceSpawns: [
                {
                    type: PieceType.CHECKER,
                    color: GameColor.BLACK,
                    position: logicalPoint({ x: 8, y: 0 }),
                },
            ],
            promotesTo: PieceType.BISHOP,
        });
    });

    it("should group multiple moves from the same fromIdx", () => {
        const paths: MovePath[] = [
            { fromIdx: 0, toIdx: 1, moveKey: "2" },
            { fromIdx: 0, toIdx: 2, moveKey: "3" },
        ];
        const boardWidth = 10;

        const result = decodePathIntoMap(paths, boardWidth);

        expect(result.size).toBe(1);
        const moves = result.get("0,0");
        expect(moves).toEqual([
            {
                ...emptyMove,
                from: { x: 0, y: 0 },
                to: { x: 1, y: 0 },
                moveKey: "2",
            },
            {
                ...emptyMove,
                from: { x: 0, y: 0 },
                to: { x: 2, y: 0 },
                moveKey: "3",
            },
        ]);
    });

    it("should return empty map when paths is empty", () => {
        const result = decodePathIntoMap([], 10);
        expect(result.size).toBe(0);
    });
});

describe("decodeEncodedMovesIntoMap", () => {
    it("should decode a valid base64 gzipped encoded move string", () => {
        const moves: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                triggerIdxs: [2],
                capturedIdxs: [3],
                intermediateIdxs: [4],
                sideEffects: [{ fromIdx: 5, toIdx: 6 }],
                pieceSpawns: [
                    {
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        posIdx: 7,
                    },
                ],
                moveKey: "1",
            },
            {
                fromIdx: 10,
                toIdx: 11,
                moveKey: "5",
            },
        ];

        const jsonString = JSON.stringify(moves);
        const compressed = brotliCompress(Buffer.from(jsonString));
        const encoded = Buffer.from(compressed).toString("base64");

        const result = decodeEncodedMovesIntoMap(encoded, 10);

        expect(result.size).toBe(2);
        expect(result.get("0,0")).toEqual<Move[]>([
            {
                from: logicalPoint({ x: 0, y: 0 }),
                to: logicalPoint({ x: 1, y: 0 }),
                moveKey: "1",
                triggers: [logicalPoint({ x: 2, y: 0 })],
                captures: [logicalPoint({ x: 3, y: 0 })],
                intermediates: [logicalPoint({ x: 4, y: 0 })],
                sideEffects: [
                    {
                        from: logicalPoint({ x: 5, y: 0 }),
                        to: logicalPoint({ x: 6, y: 0 }),
                    },
                ],
                pieceSpawns: [
                    {
                        type: PieceType.CHECKER,
                        color: GameColor.BLACK,
                        position: logicalPoint({ x: 7, y: 0 }),
                    },
                ],
                promotesTo: null,
            },
        ]);
        expect(result.get("0,1")).toEqual<Move[]>([
            {
                from: logicalPoint({ x: 0, y: 1 }),
                to: logicalPoint({ x: 1, y: 1 }),
                moveKey: "5",
                ...emptyMove,
            },
        ]);
    });

    it("should return empty map when given encoded empty move list", () => {
        const compressed = brotliCompress(Buffer.from("[]"));
        const encoded = Buffer.from(compressed).toString("base64");

        const result = decodeEncodedMovesIntoMap(encoded, 10);
        expect(result).toEqual(new Map());
    });
});
