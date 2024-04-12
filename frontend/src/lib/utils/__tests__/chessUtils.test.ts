import { PieceMap, PieceType } from "@/components/game/chess.types";
import { parseFen } from "../chessUtils";
import constants from "@/lib/constants";
import { Color } from "@/client";

describe("parseFen", () => {
    it("should parse a standard starting position correctly", () => {
        const fen =
            "rhnxqkbahr/ddpdppdpdd/c8c/10/10/10/10/C8C/DDPDPPDPDD/RHNXQKBAHR";

        const board = parseFen(fen);
        expect(board).toEqual(constants.defaultChessBoard);
    });

    it("should parse a custom position", () => {
        const fen = "4k3/8/8/3B4/4R3/8/8/4K3";
        const expectedBoard: PieceMap = new Map([
            [
                "0",
                {
                    position: [4, 0],
                    pieceType: PieceType.King,
                    color: Color.Black,
                },
            ],
            [
                "1",
                {
                    position: [3, 3],
                    pieceType: PieceType.Bishop,
                    color: Color.White,
                },
            ],
            [
                "2",
                {
                    position: [4, 4],
                    pieceType: PieceType.Rook,
                    color: Color.White,
                },
            ],
            [
                "3",
                {
                    position: [4, 7],
                    pieceType: PieceType.King,
                    color: Color.White,
                },
            ],
        ]);

        const board = parseFen(fen);
        expect(board).toEqual(expectedBoard);
    });
});
