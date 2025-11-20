import constants from "@/lib/constants";
import { GameColor, PieceType } from "@/lib/apiClient";
import { decodeFen } from "../../../chessboard/lib/fenDecoder";
import { logicalPoint } from "@/features/point/pointUtils";
import mockSequentialUUID from "@/lib/testUtils/mocks/mockUuids";
import BoardPieces from "../boardPieces";

describe("decodeFen", () => {
    it("should parse a standard starting position correctly", () => {
        mockSequentialUUID();
        const board = decodeFen(constants.INITIAL_FEN);
        expect(board).toEqual(constants.DEFAULT_CHESS_BOARD);
    });

    it("should parse a custom position", () => {
        mockSequentialUUID();

        const fen = "4k3/8/8/3B4/4R3/8/8/4K3";
        const expectedBoard = BoardPieces.fromPieces(
            {
                id: "0",
                position: logicalPoint({ x: 4, y: 0 }),
                type: PieceType.KING,
                color: GameColor.WHITE,
            },
            {
                id: "1",
                position: logicalPoint({ x: 4, y: 3 }),
                type: PieceType.ROOK,
                color: GameColor.WHITE,
            },
            {
                id: "2",
                position: logicalPoint({ x: 3, y: 4 }),
                type: PieceType.BISHOP,
                color: GameColor.WHITE,
            },
            {
                id: "3",
                position: logicalPoint({ x: 4, y: 7 }),
                type: PieceType.KING,
                color: GameColor.BLACK,
            },
        );
        const board = decodeFen(fen);
        expect(board).toEqual(expectedBoard);
    });
});
