import { type PieceMap, type Point, Color, PieceType } from "@/models";
import constants from "@/lib/constants";
import { createChessStore } from "../chessStore";

describe("position2Id", () => {
    it.each([
        // happy path
        [constants.defaultChessBoard, [0, 0], "0"],

        // no piece with the position
        [constants.defaultChessBoard, [67, 33], undefined],

        // no pieces at all
        [new Map(), [1, 2], undefined],
    ])(
        "should select the correct piece id from the position",
        (pieces, position, expectedId) => {
            const chessStore = createChessStore({ pieces: pieces as PieceMap });
            const { position2Id } = chessStore.getState();
            expect(position2Id(position as Point)).equals(expectedId);
        }
    );
});
