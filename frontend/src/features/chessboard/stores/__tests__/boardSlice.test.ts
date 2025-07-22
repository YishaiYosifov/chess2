import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import { GameColor } from "@/lib/apiClient";
import { BoardDimensions } from "../boardSlice";

describe("BoardSlice", () => {
    let store: StoreApi<ChessboardState>;

    const rect = {
        left: 100,
        top: 200,
        width: 400,
        height: 400,
    } as DOMRect;
    const boardDimensions: BoardDimensions = { width: 10, height: 10 };

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
        store.setState({ boardDimensions, boardRect: rect });
    });

    describe("screenToViewPoint / screenToLogicalPoint", () => {
        it("should return undefined when boardRect is not set", () => {
            store.setState({ boardRect: undefined });

            const state = store.getState();
            expect(state.screenToViewPoint({ x: 0, y: 0 })).toBeUndefined();
            expect(state.screenToLogicalPoint({ x: 0, y: 0 })).toBeUndefined();
        });

        it("should convert screen to view correctly", () => {
            const { screenToViewPoint, boardDimensions } = store.getState();

            // center of view‑square (2, 2)
            const screen = {
                x: rect.left + ((2 + 0.5) / boardDimensions.width) * rect.width,
                y:
                    rect.top +
                    ((2 + 0.5) / boardDimensions.height) * rect.height,
            };

            expect(screenToViewPoint(screen)).toEqual({ x: 2, y: 2 });
        });

        it("should convert screen to logical from white perspective", () => {
            store.setState({ viewingFrom: GameColor.WHITE });

            // very top‑left pixel of the board
            const logical = store
                .getState()
                .screenToLogicalPoint({ x: rect.left + 1, y: rect.top + 1 });

            expect(logical).toEqual({ x: 0, y: boardDimensions.height - 1 });
        });

        it("should convert screen to logical from black perspective", () => {
            store.setState({ viewingFrom: GameColor.BLACK });

            const logical = store
                .getState()
                .screenToLogicalPoint({ x: rect.left + 1, y: rect.top + 1 });

            expect(logical).toEqual({ x: boardDimensions.width - 1, y: 0 });
        });

        it("should round‑trip correctly for white", () => {
            store.setState({ viewingFrom: GameColor.WHITE });

            const { viewPointToLogicalPoint, logicalPointToViewPoint } =
                store.getState();

            const view = { x: 3, y: 2 };
            const logical = viewPointToLogicalPoint(view);

            expect(logicalPointToViewPoint(logical)).toEqual(view);
        });

        it("should round‑trip correctly for black", () => {
            store.setState({ viewingFrom: GameColor.BLACK });

            const { viewPointToLogicalPoint, logicalPointToViewPoint } =
                store.getState();

            const view = { x: 3, y: 2 };
            const logical = viewPointToLogicalPoint(view);

            expect(logicalPointToViewPoint(logical)).toEqual(view);
        });
    });

    describe("logicalPointToScreenPoint", () => {
        it("should return undefined when boardRect is missing", () => {
            store.setState({ boardRect: undefined });

            expect(
                store.getState().logicalPointToScreenPoint({ x: 0, y: 0 }),
            ).toBeUndefined();
        });

        it("should map logical to screen for white", () => {
            store.setState({ viewingFrom: GameColor.WHITE });

            const point = store.getState().logicalPointToScreenPoint({
                x: 0,
                y: boardDimensions.height - 1,
            })!;
            const expectedX =
                rect.left + ((0 + 0.5) / boardDimensions.width) * rect.width;
            const expectedY =
                rect.top + ((0 + 0.5) / boardDimensions.height) * rect.height;

            expect(point.x).toBeCloseTo(expectedX);
            expect(point.y).toBeCloseTo(expectedY);
        });

        it("should map logical to screen for black", () => {
            store.setState({ viewingFrom: GameColor.BLACK });

            const point = store.getState().logicalPointToScreenPoint({
                x: boardDimensions.width - 1,
                y: 0,
            })!;

            const expectedX =
                rect.left + ((0 + 0.5) / boardDimensions.width) * rect.width;
            const expectedY =
                rect.top + ((0 + 0.5) / boardDimensions.height) * rect.height;

            expect(point.x).toBeCloseTo(expectedX);
            expect(point.y).toBeCloseTo(expectedY);
        });
    });

    describe("setBoardRect", () => {
        it("persists boardRect in state", () => {
            const r = { ...rect } as DOMRect;
            store.getState().setBoardRect(r);

            expect(store.getState().boardRect).toBe(r);
        });
    });
});
