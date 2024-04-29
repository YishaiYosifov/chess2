import { type PieceMap, type Point, Color, PieceType } from "@/models";
import { createChessStore } from "../chessStore";

describe("position2Id", () => {
    it.each([
        [
            new Map([
                [
                    "1",
                    {
                        position: [1, 2],
                        pieceType: PieceType.King,
                        color: Color.White,
                    },
                ],
            ]),
            [1, 2],
            "1",
        ],
        [
            new Map([
                [
                    "1",
                    {
                        position: [1, 2],
                        pieceType: PieceType.King,
                        color: Color.White,
                    },
                ],
            ]),
            [1, 3],
            undefined,
        ],
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
