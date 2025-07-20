import brotliCompress from "brotli/compress";

import { MovePath } from "@/lib/apiClient";
import { decodeEncodedMovesIntoMap, decodePathIntoMap } from "../moveDecoder";
import { gzipSync } from "zlib";

const emptyMove = {
    triggers: [],
    captures: [],
    sideEffects: [],
};

describe("decodePathIntoMap", () => {
    it("should decode single path into correct LegalMoveMap entry", () => {
        const paths: MovePath[] = [
            {
                fromIdx: 0,
                toIdx: 1,
                triggerIdxs: [2],
                capturedIdxs: [3],
                sideEffects: [{ fromIdx: 4, toIdx: 5 }],
            },
        ];

        const result = decodePathIntoMap(paths, 10);

        expect(result.size).toBe(1);
        const moves = result.get("0,0");
        expect(moves).toBeDefined();
        expect(moves).toHaveLength(1);

        const move = moves![0];
        expect(move).toEqual({
            from: { x: 0, y: 0 },
            to: { x: 1, y: 0 },
            triggers: [{ x: 2, y: 0 }],
            captures: [{ x: 3, y: 0 }],
            sideEffects: [{ from: { x: 4, y: 0 }, to: { x: 5, y: 0 } }],
        });
    });

    it("should group multiple moves from the same fromIdx", () => {
        const paths: MovePath[] = [
            { fromIdx: 0, toIdx: 1 },
            { fromIdx: 0, toIdx: 2 },
        ];
        const boardWidth = 10;

        const result = decodePathIntoMap(paths, boardWidth);

        expect(result.size).toBe(1);
        const moves = result.get("0,0");
        expect(moves).toEqual([
            { ...emptyMove, from: { x: 0, y: 0 }, to: { x: 1, y: 0 } },
            { ...emptyMove, from: { x: 0, y: 0 }, to: { x: 2, y: 0 } },
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
                sideEffects: [{ fromIdx: 4, toIdx: 5 }],
            },
            {
                fromIdx: 10,
                toIdx: 11,
            },
        ];

        const jsonString = JSON.stringify(moves);
        const gzipped = brotliCompress(Buffer.from(jsonString));
        const encoded = Buffer.from(gzipped).toString("base64");

        const result = decodeEncodedMovesIntoMap(encoded, 10);

        expect(result.size).toBe(2);
        expect(result.get("0,0")).toEqual([
            {
                from: { x: 0, y: 0 },
                to: { x: 1, y: 0 },
                triggers: [{ x: 2, y: 0 }],
                captures: [{ x: 3, y: 0 }],
                sideEffects: [{ from: { x: 4, y: 0 }, to: { x: 5, y: 0 } }],
            },
        ]);
        expect(result.get("0,1")).toEqual([
            {
                from: { x: 0, y: 1 },
                to: { x: 1, y: 1 },
                ...emptyMove,
            },
        ]);
    });

    it("should return empty map when given encoded empty move list", () => {
        const emptyEncoded = gzipSync(Buffer.from("[]")).toString("base64");
        const result = decodeEncodedMovesIntoMap(emptyEncoded, 10);
        expect(result).toEqual(new Map());
    });
});
