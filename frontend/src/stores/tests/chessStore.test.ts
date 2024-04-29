import { PieceID, PieceMap, type LegalMoves, type Point } from "@/models";
import { createChessStore } from "../chessStore";
import constants from "@/lib/constants";

describe("movePiece", () => {
    it.each<[PieceMap, from: Point, to: Point]>([
        // happy path
        [constants.defaultChessBoard, [0, 0], [69, 420]],

        // captures
        [constants.defaultChessBoard, [0, 0], [1, 2]],

        // piece not found
        [new Map(), [0, 0], [1, 2]],
    ])("should correctly move and capture pieces", (pieces, from, to) => {
        const chessStore = createChessStore({ pieces });

        const { movePiece, position2Id } = chessStore.getState();
        const fromId = position2Id(from);
        const toId = position2Id(to);

        movePiece(from, to);

        const { pieces: newPieces } = chessStore.getState();

        const movedPiece = newPieces.get(fromId!);
        const capturedPiece = newPieces.get(toId!);

        // if the piece does not exist, it should still not exist after moving it
        if (fromId) expect(movedPiece?.position).toEqual(to);
        else expect(movedPiece).toBeUndefined();

        expect(capturedPiece).toBeUndefined();
    });
});

describe("position2Id", () => {
    it.each<[PieceMap, Point, PieceID | undefined]>([
        // happy path
        [constants.defaultChessBoard, [0, 0], "0"],

        // no piece with the position
        [constants.defaultChessBoard, [67, 33], undefined],

        // no pieces at all
        [new Map(), [1, 2], undefined],
    ])(
        "should select the correct piece id from the position",
        (pieces, position, expectedId) => {
            const chessStore = createChessStore({ pieces });
            const { position2Id } = chessStore.getState();
            expect(position2Id(position)).toEqual(expectedId);
        }
    );
});

describe("showLegalMoves", () => {
    it.each<[LegalMoves, Point, Point[]]>([
        // happy path
        [
            {
                "3,3": ["3,4", "3,5"],
                "4,4": ["5,4", "6,4"],
            },
            [3, 3],
            [
                [3, 4],
                [3, 5],
            ],
        ],

        // no legal moves for a position
        [
            {
                "3,3": ["1,2"],
            },
            [1, 2],
            [],
        ],

        // no legal moves at all
        [{}, [1, 1], []],
    ])(
        "should highlight the correct squares depending on the position",
        (legalMoves, position, expectedLegalMoves) => {
            const chessStore = createChessStore({ legalMoves });
            const { showLegalMoves } = chessStore.getState();

            showLegalMoves(position);
            const { highlightedLegalMoves, selectedPiecePosition } =
                chessStore.getState();
            expect(highlightedLegalMoves).toEqual(expectedLegalMoves);
            expect(selectedPiecePosition).toEqual(position);
        }
    );
});
