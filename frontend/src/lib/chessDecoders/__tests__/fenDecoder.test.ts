import { PieceMap, PieceType } from "@/types/tempModels";
import constants from "@/lib/constants";
import { GameColor } from "@/lib/apiClient";
import { decodeFen } from "../fenDecoder";

describe("decodeFen", () => {
    it("should parse a standard starting position correctly", () => {
        const fen =
            "rh2qkb1hr/pp1pppp1pp/10/10/10/10/10/10/PP1PPPP1PP/RH2QKB1HR";

        const board = decodeFen(fen);
        expect(board).toEqual(constants.DEFAULT_CHESS_BOARD);
    });

    it("should parse a custom position", () => {
        const fen = "4k3/8/8/3B4/4R3/8/8/4K3";
        const expectedBoard: PieceMap = new Map([
            [
                "0",
                {
                    position: [4, 0],
                    type: PieceType.KING,
                    color: GameColor.BLACK,
                },
            ],
            [
                "1",
                {
                    position: [3, 3],
                    type: PieceType.BISHOP,
                    color: GameColor.WHITE,
                },
            ],
            [
                "2",
                {
                    position: [4, 4],
                    type: PieceType.ROOK,
                    color: GameColor.WHITE,
                },
            ],
            [
                "3",
                {
                    position: [4, 7],
                    type: PieceType.KING,
                    color: GameColor.WHITE,
                },
            ],
        ]);

        const board = decodeFen(fen);
        expect(board).toEqual(expectedBoard);
    });
});
