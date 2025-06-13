import {
    LegalMoveMap,
    Move,
    PieceID,
    PieceMap,
    StrPoint,
    type Point,
} from "@/types/tempModels";
import { createChessboardStore } from "../chessboardStore";
import constants from "@/lib/constants";

describe("movePiece", () => {
    it.each<[PieceMap, from: Point, to: Point]>([
        // happy path
        [constants.DEFAULT_CHESS_BOARD, { x: 0, y: 0 }, { x: 69, y: 420 }],

        // captures
        [constants.DEFAULT_CHESS_BOARD, { x: 0, y: 0 }, { x: 1, y: 2 }],

        // piece not found
        [new Map(), { x: 0, y: 0 }, { x: 1, y: 2 }],
    ])("should correctly move and capture pieces", (pieces, from, to) => {
        const chessStore = createChessboardStore({ pieces });

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
        [constants.DEFAULT_CHESS_BOARD, { x: 0, y: 0 }, "0"],

        // no piece with the position
        [constants.DEFAULT_CHESS_BOARD, { x: 67, y: 33 }, undefined],

        // no pieces at all
        [new Map(), { x: 1, y: 2 }, undefined],
    ])(
        "should select the correct piece id from the position",
        (pieces, position, expectedId) => {
            const chessStore = createChessboardStore({ pieces });
            const { position2Id } = chessStore.getState();
            expect(position2Id(position)).toEqual(expectedId);
        },
    );
});

describe("showLegalMoves", () => {
    const emptyMove = { captures: [], through: [], sideEffects: [] };
    it.each<[LegalMoveMap, Point, Point[]]>([
        // happy path
        [
            new Map<StrPoint, Move[]>([
                [
                    "3,3",
                    [
                        {
                            ...emptyMove,
                            from: { x: 3, y: 3 },
                            to: { x: 3, y: 4 },
                        },
                        {
                            ...emptyMove,
                            from: { x: 3, y: 3 },
                            to: { x: 3, y: 5 },
                        },
                    ],
                ],
                [
                    "4,4",
                    [
                        {
                            ...emptyMove,
                            from: { x: 4, y: 4 },
                            to: { x: 5, y: 4 },
                        },
                        {
                            ...emptyMove,
                            from: { x: 4, y: 4 },
                            to: { x: 6, y: 4 },
                        },
                    ],
                ],
            ]),
            { x: 3, y: 3 },
            [
                { x: 3, y: 4 },
                { x: 3, y: 5 },
            ],
        ],

        // no legal moves for a position
        [
            new Map([
                [
                    "3,3",
                    [
                        {
                            ...emptyMove,
                            from: { x: 3, y: 3 },
                            to: { x: 1, y: 2 },
                        },
                    ],
                ],
            ]),
            { x: 1, y: 2 },
            [],
        ],

        // no legal moves at all
        [new Map(), { x: 1, y: 1 }, []],
    ])(
        "should highlight the correct squares depending on the position",
        (legalMoves, position, expectedLegalMoves) => {
            const chessStore = createChessboardStore({ legalMoves });
            const { showLegalMoves } = chessStore.getState();

            showLegalMoves(position);
            const { highlightedLegalMoves, selectedPiecePosition } =
                chessStore.getState();
            expect(highlightedLegalMoves).toEqual(expectedLegalMoves);
            expect(selectedPiecePosition).toEqual(position);
        },
    );
});
