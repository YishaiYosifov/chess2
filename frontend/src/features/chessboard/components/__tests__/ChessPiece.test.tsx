import { render, screen } from "@testing-library/react";

import { LogicalPoint } from "@/features/point/types";
import { Point } from "@/features/point/types";
import { Move } from "../../lib/types";
import { LegalMoveMap } from "../../lib/types";
import { PieceMap } from "../../lib/types";
import ChessboardStoreContext from "@/features/chessboard/contexts/chessboardStoreContext";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import ChessboardLayout from "../ChessboardLayout";
import { mockBoundingClientRect } from "@/lib/testUtils/mocks/mockDom";
import { logicalPoint, pointToStr } from "@/features/point/pointUtils";
import {
    createFakeMove,
    createFakePiece,
} from "@/lib/testUtils/fakers/chessboardFakers";
import { createMoveOptions } from "../../lib/moveOptions";
import getPieceImage from "../../lib/pieceImage";

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

    let store: StoreApi<ChessboardStore>;

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
        const pieceMap: PieceMap = new Map([["0", pieceInfo]]);
        store.setState({
            pieceMap,
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
            background-image: url("${getPieceImage(pieceInfo.type, pieceInfo.color)}");
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
        const move = createFakeMove({
            from: startPos,
            to: destinationPos,
        });
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

        const pieceMap = store.getState().pieceMap;
        expect(pieceMap.get("0")?.position).toEqual(move.to);
        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 700, y: 400 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should move the piece when clicked on it, move the mouse and click on the destination", async () => {
        const { logicalPointToScreenPoint } = store.getState();

        const startPos = logicalPoint({ x: 0, y: 9 });
        const destinationPos = logicalPoint({ x: 2, y: 3 });
        const move: Move = createFakeMove({
            from: startPos,
            to: destinationPos,
        });

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

        const pieceMap = store.getState().pieceMap;
        expect(pieceMap.get("0")?.position).toEqual(move.to);
        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 200, y: 600 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should prioritize animatingPieceMap over regular pieces", () => {
        const pieceId = "0";
        const normalPiece = createFakePiece({
            position: logicalPoint({ x: 0, y: 0 }),
        });
        const animatingPiece = createFakePiece({
            position: logicalPoint({ x: 7, y: 3 }),
        });

        const pieceMap: PieceMap = new Map([[pieceId, normalPiece]]);
        const animatingPieceMap: PieceMap = new Map([
            [pieceId, animatingPiece],
        ]);

        store.setState({
            pieceMap,
            animatingPieceMap,
        });

        const { piece } = renderPiece({
            logicalPosition: normalPiece.position,
        });

        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 700, y: 600 }, // based on intermediate position { x: 7, y: 3 }
        });

        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it.each([true, false])(
        "should set opacity to 50% when the piece is being removed",
        (isRemoving) => {
            const piece = createFakePiece();
            const pieceId = "0";

            store.setState({
                removingPieces: isRemoving ? new Set([pieceId]) : new Set(),
            });

            const { piece: renderedPiece } = renderPiece({
                logicalPosition: piece.position,
            });

            expect(renderedPiece.classList.contains("opacity-50")).toBe(
                isRemoving,
            );
        },
    );

    it("should not allow dragging if canDrag is false", async () => {
        const { logicalPointToScreenPoint } = store.getState();
        store.setState({ canDrag: false });

        const user = userEvent.setup();
        const { piece, pieceInfo, chessboard } = renderPiece();

        await user.pointer([
            {
                target: chessboard,
                coords: logicalPointToScreenPoint(pieceInfo.position),
                keys: "[MouseLeft>]",
            },
            { coords: { x: 100, y: 100 } },
        ]);
        vi.advanceTimersToNextFrame();

        const expectedTransform = getExpectedTransform({
            percentPosition: { x: 0, y: 0 },
        });
        expect(normalize(piece.style.transform)).toBe(expectedTransform);
    });

    it("should show the double click indicator when a double click occurs", async () => {
        const user = userEvent.setup();

        const startPos = logicalPoint({ x: 0, y: 9 });
        const move = createFakeMove({
            from: startPos,
            to: startPos,
        });
        const legalMoves: LegalMoveMap = new Map([
            [pointToStr(startPos), [move]],
        ]);

        const { chessboard } = renderPiece({
            logicalPosition: startPos,
            legalMoves,
        });

        const coords = store.getState().logicalPointToScreenPoint(startPos);
        await user.pointer([
            {
                target: chessboard,
                coords,
                keys: "[MouseLeft>]",
            },
            {
                target: chessboard,
                coords,
                keys: "[/MouseLeft]",
            },
        ]);
        vi.advanceTimersToNextFrame();

        const indicator = screen.getByTestId("doubleClickIndicator");
        expect(indicator).toBeInTheDocument();
        expect(indicator).toBeVisible();
    });
});
