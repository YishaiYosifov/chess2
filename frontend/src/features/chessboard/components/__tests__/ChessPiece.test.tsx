import { render, screen } from "@testing-library/react";

import {
    PieceMap,
    Point,
    LegalMoveMap,
    Move,
    LogicalPoint,
} from "@/types/tempModels";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";
import {
    ChessboardState,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardLayout from "../ChessboardLayout";
import { mockBoundingClientRect } from "@/lib/testUtils/mocks/mockWindow";
import { logicalPoint, pointToStr } from "@/lib/utils/pointUtils";
import { createFakePiece } from "@/lib/testUtils/fakers/chessboardFakers";
import { createMoveOptions } from "../../lib/moveOptions";

describe("ChessPiece", () => {
    const normalize = (str: string) => str.replace(/\s+/g, "");

    const boardRect = {
        x: 0,
        y: 0,
        width: 768,
        height: 768,
        top: 0,
        right: 768,
        bottom: 768,
        left: 0,
    } as DOMRect;
    const pieceRect = {
        x: 2,
        y: 2,
        width: 70,
        height: 70,
        top: 2,
        right: 80,
        bottom: 78,
        left: 2,
    } as DOMRect;
    const expectedCenterOffset: Point = {
        x: pieceRect.left + pieceRect.width / 2,
        y: pieceRect.top + pieceRect.height / 2,
    };

    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        store = createChessboardStore();
        vi.useFakeTimers({ toFake: ["requestAnimationFrame"] });
        mockBoundingClientRect({
            chessboard: boardRect,
            piece: pieceRect,
        });
    });

    function getExpectedTransform({
        percentPosition,
        draggingOffset,
        centerOffset,
    }: {
        percentPosition: Point;
        draggingOffset?: Point;
        centerOffset?: Point;
    }) {
        draggingOffset ??= { x: 0, y: 0 };
        centerOffset ??= { x: 0, y: 0 };
        const expected = `translate(
                clamp(0%, calc(${percentPosition.x}% + ${draggingOffset.x - centerOffset.x}px), 900%),
                clamp(0%, calc(${percentPosition.y}% + ${draggingOffset.y - centerOffset.y}px), 900%))`;
        return normalize(expected);
    }

    function renderPiece({
        logicalPosition,
        legalMoves,
    }: { logicalPosition?: LogicalPoint; legalMoves?: LegalMoveMap } = {}) {
        logicalPosition ??= logicalPoint({ x: 0, y: 9 });
        legalMoves ??= new Map();

        const pieceInfo = createFakePiece({ position: logicalPosition });
        const pieces: PieceMap = new Map([["0", pieceInfo]]);
        store.setState({
            pieces,
            moveOptions: createMoveOptions({ legalMoves }),
        });

        const renderResults = render(
            <ChessboardStoreContext.Provider value={store}>
                <ChessboardLayout />
            </ChessboardStoreContext.Provider>,
        );
        const piece = screen.getByTestId("piece");
        const chessboard = screen.getByTestId("chessboard");

        return {
            ...renderResults,
            piece,
            pieceInfo,
            chessboard,
        };
    }

    it.each([
        [logicalPoint({ x: 0, y: 0 }), { x: 0, y: 900 }],
        [logicalPoint({ x: 1, y: 1 }), { x: 100, y: 800 }],
        [logicalPoint({ x: 0, y: 5 }), { x: 0, y: 400 }],
    ])(
        "should be in the correct position",
        (logicalPosition, percentPosition) => {
            const { pieceInfo, piece } = renderPiece({
                logicalPosition: logicalPosition,
            });

            const expectedTransform = getExpectedTransform({ percentPosition });
            expect(piece).toHaveStyle(`
            background-image: url("/assets/pieces/${pieceInfo.type}${pieceInfo.color}.png");
        `);
            expect(normalize(piece.style.transform)).toBe(expectedTransform);
        },
    );

    it("should snap to the mouse when clicked", async () => {
        const { logicalPointToScreenPoint } = store.getState();
        const mouseCoords = { x: 1, y: 2 };

        const user = userEvent.setup();
        const { piece, pieceInfo, chessboard } = renderPiece();

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(pieceInfo.position),
                keys: "[MouseLeft>]",
            },
            { coords: mouseCoords },
        ]);
        vi.advanceTimersToNextFrame();

        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 0, y: 0 },
            draggingOffset: mouseCoords,
            centerOffset: expectedCenterOffset,
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should follow the mouse after clicking", async () => {
        const { logicalPointToScreenPoint } = store.getState();
        const mouseCoords = { x: 69, y: 420 };

        const user = userEvent.setup();
        const { piece, pieceInfo, chessboard } = renderPiece();

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(pieceInfo.position),
                keys: "[MouseLeft>]",
            },
            { coords: mouseCoords },
        ]);
        vi.advanceTimersToNextFrame();

        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 0, y: 0 },
            draggingOffset: mouseCoords,
            centerOffset: expectedCenterOffset,
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should release reset the position of the piece once released", async () => {
        const { logicalPointToScreenPoint } = store.getState();
        const user = userEvent.setup();
        const { piece, pieceInfo, chessboard } = renderPiece();

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(pieceInfo.position),
                keys: "[MouseLeft>]",
            },
            { coords: { x: 6, y: 9 } },
            { keys: "[/MouseLeft]" },
        ]);

        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 0, y: 0 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should move the piece to a legal square when clicked", async () => {
        const { logicalPointToScreenPoint } = store.getState();

        const startPos = logicalPoint({ x: 0, y: 9 });
        const destinationPos = logicalPoint({ x: 7, y: 5 });
        const move: Move = {
            from: startPos,
            to: destinationPos,
            triggers: [],
            captures: [],
            sideEffects: [],
        };
        const legalMoves: LegalMoveMap = new Map([
            [pointToStr(startPos), [move]],
        ]);

        const user = userEvent.setup();
        const { chessboard, piece } = renderPiece({
            logicalPosition: startPos,
            legalMoves,
        });

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(startPos),
                keys: "[MouseLeft>]",
            },
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(move.to),
                keys: "[/MouseLeft]",
            },
        ]);
        vi.advanceTimersToNextFrame();

        const pieces = store.getState().pieces;
        expect(pieces.get("0")?.position).toEqual(move.to);
        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 500, y: 200 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should move the piece when clicked on it, move the mouse and click on the destination", async () => {
        const { logicalPointToScreenPoint } = store.getState();

        const startPos = logicalPoint({ x: 0, y: 9 });
        const destinationPos = logicalPoint({ x: 2, y: 3 });
        const move: Move = {
            from: startPos,
            to: destinationPos,
            triggers: [],
            captures: [],
            sideEffects: [],
        };

        const legalMoves: LegalMoveMap = new Map([
            [pointToStr(startPos), [move]],
        ]);

        const user = userEvent.setup();

        const { chessboard, piece } = renderPiece({
            logicalPosition: startPos,
            legalMoves,
        });

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(startPos),
                keys: "[MouseLeft]",
            },
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(destinationPos),
                keys: "[MouseLeft]",
            },
        ]);
        vi.advanceTimersToNextFrame();

        const pieces = store.getState().pieces;
        expect(pieces.get("0")?.position).toEqual(move.to);
        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 200, y: 600 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });
});
