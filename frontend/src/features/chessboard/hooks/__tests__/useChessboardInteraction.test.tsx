import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "../../stores/chessboardStore";
import useBoardInteraction from "../useBoardInteraction";
import ChessboardStoreContext from "../../contexts/chessboardStoreContext";
import { act, renderHook } from "@testing-library/react";

function createMouseEvent(
    x = 100,
    y = 200,
    button = 0,
): React.MouseEvent<HTMLElement, MouseEvent> {
    return {
        clientX: x,
        clientY: y,
        button,
    } as unknown as React.MouseEvent<HTMLElement, MouseEvent>;
}

describe("useBoardInteraction", () => {
    let store: StoreApi<ChessboardStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
    });

    const wrapper = ({ children }: { children: React.ReactNode }) => (
        <ChessboardStoreContext.Provider value={store}>
            {children}
        </ChessboardStoreContext.Provider>
    );

    it("should call onPress on pointer down", async () => {
        const onPress = vi.fn();
        renderHook(
            () =>
                useBoardInteraction({
                    shouldStartDrag: vi.fn().mockResolvedValue(false),
                    onPress,
                }),
            { wrapper },
        );

        await act(() =>
            store.getState().onPointerDown(createMouseEvent(10, 20)),
        );

        expect(onPress).toHaveBeenCalledWith({
            point: { x: 10, y: 20 },
            button: 0,
        });
    });

    it("should call onClick on pointer up when not dragging", async () => {
        const onClick = vi.fn();
        renderHook(
            () =>
                useBoardInteraction({
                    shouldStartDrag: vi.fn().mockResolvedValue(false),
                    onClick,
                }),
            { wrapper },
        );

        await act(async () => {
            await store.getState().onPointerDown(createMouseEvent(0, 0));
            await store.getState().onPointerUp(createMouseEvent(50, 60));
        });

        expect(onClick).toHaveBeenCalledWith({
            point: { x: 50, y: 60 },
            button: 0,
        });
    });

    it("should call onDragStart and onDragEnd", async () => {
        const onDragStart = vi.fn();
        const onDragEnd = vi.fn();
        renderHook(
            () =>
                useBoardInteraction({
                    shouldStartDrag: vi.fn().mockResolvedValue(true),
                    onDragStart,
                    onDragEnd,
                }),
            { wrapper },
        );

        const down = createMouseEvent(5, 5);

        await act(() => store.getState().onPointerDown(down));
        await act(async () => {
            const up = new MouseEvent("pointerup", {
                clientX: 30,
                clientY: 40,
            });

            window.dispatchEvent(up);
        });

        expect(onDragStart).toHaveBeenCalledWith({ x: 5, y: 5 });
        expect(onDragEnd).toHaveBeenCalledWith({ x: 30, y: 40 });
    });

    it("should call onDragMove during pointermove", async () => {
        const onDragMove = vi.fn();
        const shouldStartDrag = vi.fn().mockResolvedValue(true);
        renderHook(
            () =>
                useBoardInteraction({
                    shouldStartDrag,
                    onDragMove,
                }),
            { wrapper },
        );

        await act(() => store.getState().onPointerDown(createMouseEvent(0, 0)));

        await act(async () => {
            const moveEvent = new MouseEvent("pointermove", {
                clientX: 123,
                clientY: 456,
            });
            window.dispatchEvent(moveEvent);
            vi.runAllTimers();
        });

        expect(onDragMove).toHaveBeenCalledWith({ x: 123, y: 456 });
    });

    it("should track isDragging correctly", async () => {
        const shouldStartDrag = vi.fn().mockResolvedValue(true);
        const { result } = renderHook(
            () =>
                useBoardInteraction({
                    shouldStartDrag,
                }),
            { wrapper },
        );

        expect(result.current).toBe(false);
        await act(() => store.getState().onPointerDown(createMouseEvent(0, 0)));
        expect(result.current).toBe(true);

        await act(async () => {
            const up = new MouseEvent("pointerup", {
                clientX: 0,
                clientY: 0,
            });
            window.dispatchEvent(up);
        });

        expect(result.current).toBe(false);
    });
});
