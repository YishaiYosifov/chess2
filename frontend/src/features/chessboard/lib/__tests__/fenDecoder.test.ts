import { PieceMap } from "@/features/chessboard/lib/types";
import constants from "@/lib/constants";
import { GameColor, PieceType } from "@/lib/apiClient";
import { decodeFen } from "../../../chessboard/lib/fenDecoder";
import { logicalPoint } from "@/features/point/pointUtils";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";

describe("decodeFen", () => {
    it("should parse a standard starting position correctly", () => {
        const board = decodeFen(constants.INITIAL_FEN);
        expect(board.values()).toEqual(constants.DEFAULT_CHESS_BOARD.values());
    });

    it("should parse a custom position", () => {
        mockSequentialUUID();

        const fen = "4k3/8/8/3B4/4R3/8/8/4K3";
        const expectedBoard: PieceMap = new Map([
            [
                "0",
                {
                    position: logicalPoint({ x: 4, y: 0 }),
                    type: PieceType.KING,
                    color: GameColor.WHITE,
                },
            ],
            [
                "1",
                {
                    position: logicalPoint({ x: 4, y: 3 }),
                    type: PieceType.ROOK,
                    color: GameColor.WHITE,
                },
            ],
            [
                "2",
                {
                    position: logicalPoint({ x: 3, y: 4 }),
                    type: PieceType.BISHOP,
                    color: GameColor.WHITE,
                },
            ],
            [
                "3",
                {
                    position: logicalPoint({ x: 4, y: 7 }),
                    type: PieceType.KING,
                    color: GameColor.BLACK,
                },
            ],
        ]);
        const board = decodeFen(fen);
        expect(board).toEqual(expectedBoard);
    });
});
