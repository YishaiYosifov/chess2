import { pointToString } from "@/lib/utils/pointUtils";
import { decodeLegalMoves } from "../moveDecoder";

describe("decodeLegalMoves", () => {
    const emptyMove = {
        through: [],
        captures: [],
        sideEffects: [],
    };

    it("should decode a simple move without captures or sideEffects", () => {
        const encoded = ["e2e4"];
        const moves = decodeLegalMoves(encoded);

        const expectedKey = pointToString([4, 1]); // e2

        expect(moves.has(expectedKey)).toBe(true);
        const moveArr = moves.get(expectedKey);

        expect(moveArr).toHaveLength(1);

        const move = moveArr![0];
        expect(move).toEqual({
            ...emptyMove,
            from: [4, 1], // e2
            to: [4, 3], // e4
        });
    });

    it("should decode a move with captures", () => {
        const encoded = ["e4d5!d5"];
        const moves = decodeLegalMoves(encoded);

        const expectedKey = pointToString([4, 3]); // e4
        const move = moves.get(expectedKey)![0];

        expect(move).toEqual({
            ...emptyMove,
            from: [4, 3], // e4
            to: [3, 4], // d5
            captures: [[3, 4]], // d5
        });
    });

    it("should decode a move with through points", () => {
        const encoded = ["e2d3c4"];
        const moves = decodeLegalMoves(encoded);

        const expectedFrom = pointToString([4, 1]); // e2
        const move = moves.get(expectedFrom)![0];

        expect(move).toEqual({
            ...emptyMove,
            from: [4, 1], // e2
            through: [[3, 2]], // d3
            to: [2, 3], // c4
        });
    });

    it("should decode a move with sideEffects", () => {
        const encoded = ["e2d3c4-d3c4d5"];
        const moves = decodeLegalMoves(encoded);

        const move = moves.get(pointToString([4, 1]))![0]; // e2
        expect(move).toEqual({
            ...emptyMove,
            from: [4, 1], //e2
            through: [[3, 2]], // d3
            to: [2, 3], // c4
            sideEffects: [
                {
                    ...emptyMove,
                    from: [3, 2], // d3
                    through: [[2, 3]], // c4
                    to: [3, 4], // d5
                },
            ],
        });
    });

    it("should decode a move with sideEffects and captures", () => {
        // main move: e2d3c4 capturing d3
        // side effect: d3c4d5 capturing c4
        const encoded = ["e2c4!d3-d3c4d5!c4!c5"];
        const moves = decodeLegalMoves(encoded);

        const move = moves.get(pointToString([4, 1]))![0];
        expect(move.from).toEqual([4, 1]);
        expect(move.to).toEqual([2, 3]);
        expect(move.captures).toEqual([[3, 2]]); // d3
        expect(move).toEqual({
            ...emptyMove,
            from: [4, 1], // e2
            to: [2, 3], // d3
            captures: [[3, 2]], // d3
            sideEffects: [
                {
                    ...emptyMove,
                    from: [3, 2], // d3
                    through: [[2, 3]], // c4
                    to: [3, 4], // d5
                    captures: [
                        [2, 3],
                        [2, 4],
                    ], // c4, c5
                },
            ],
        });
    });

    it("should throw an error on invalid move (too few points)", () => {
        const invalid = ["e2"];
        expect(() => decodeLegalMoves(invalid)).toThrow(/not enough points/);
    });

    it("should throw an error on completely invalid input", () => {
        const invalid = ["xyz"];
        expect(() => decodeLegalMoves(invalid)).toThrow(
            /could not parse points/,
        );
    });
});
