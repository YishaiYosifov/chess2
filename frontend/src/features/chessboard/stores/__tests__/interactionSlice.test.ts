import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import { InteractionInfo } from "../interactionSlice";
import { screenPoint } from "@/lib/utils/pointUtils";

describe("InteractionSlice", () => {
    let store: StoreApi<ChessboardState>;

    beforeEach(() => {
        vi.useFakeTimers();
        store = createChessboardStore();
    });

    function mockMouseEvent(
        x = 0,
        y = 0,
        button = 0,
    ): React.MouseEvent<HTMLElement, MouseEvent> {
        return {
            clientX: x,
            clientY: y,
            button,
        } as unknown as React.MouseEvent<HTMLElement, MouseEvent>;
    }

    describe("onPointerDown", () => {
        it("should set interaction state and emit pointerDown/dragStart events in order", async () => {
            const pointerDownSpy = vi.fn();
            const dragStartSpy = vi.fn().mockResolvedValue(true);

            store.getState().pointerDownEvent.subscribe(pointerDownSpy);
            store.getState().dragStartQuery.subscribe(dragStartSpy);

            const evt = mockMouseEvent(50, 60, 2);

            await store.getState().onPointerDown(evt);

            const expectedInfo: InteractionInfo = {
                point: screenPoint({ x: 50, y: 60 }),
                button: 2,
            };

            // state updated
            expect(store.getState().interaction).toEqual(expectedInfo);

            // events emitted with the same payload
            expect(pointerDownSpy).toHaveBeenCalledTimes(1);
            expect(pointerDownSpy).toHaveBeenCalledWith(expectedInfo);

            expect(dragStartSpy).toHaveBeenNthCalledWith(1, expectedInfo);

            // make sure pointerDown fires before dragStart
            const pointerDownOrder = pointerDownSpy.mock.invocationCallOrder[0];
            const dragStartOrder = dragStartSpy.mock.invocationCallOrder[0];
            expect(pointerDownOrder).toBeLessThan(dragStartOrder);
        });
    });

    describe("onPointerUp", () => {
        it("should clear interaction state and emit pointerUp event", async () => {
            store.setState({
                interaction: { point: screenPoint({ x: 1, y: 1 }), button: 1 },
            });

            const pointerUpSpy = vi.fn();
            store.getState().pointerUpEvent.subscribe(pointerUpSpy);

            const evt = mockMouseEvent(100, 200, 0);

            await store.getState().onPointerUp(evt);

            const expectedInfo: InteractionInfo = {
                point: screenPoint({ x: 100, y: 200 }),
                button: 0,
            };

            expect(store.getState().interaction).toBeNull();
            expect(pointerUpSpy).toHaveBeenCalledTimes(1);
            expect(pointerUpSpy).toHaveBeenCalledWith(expectedInfo);
        });
    });

    describe("evaluateDragStart", () => {
        it("should stop at the first listener that returns truthy", async () => {
            const first = vi.fn().mockResolvedValue(false);
            const second = vi.fn().mockResolvedValue(true);
            const third = vi.fn().mockResolvedValue(true);

            const { dragStartQuery } = store.getState();
            dragStartQuery.subscribe(first);
            dragStartQuery.subscribe(second);
            dragStartQuery.subscribe(third);

            const info: InteractionInfo = {
                point: screenPoint({ x: 0, y: 0 }),
                button: 0,
            };

            await store.getState().evaluateDragStart(info);

            expect(first).toHaveBeenCalledTimes(1);
            expect(second).toHaveBeenCalledTimes(1);
            expect(third).not.toHaveBeenCalled();
        });
    });
});
