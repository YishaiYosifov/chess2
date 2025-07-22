import { StoreApi } from "zustand";
import { ChessboardState, createChessboardStore } from "../chessboardStore";
import { InteractionInfo } from "../interactionSlice";

describe("InteractionSlice", () => {
    let store: StoreApi<ChessboardState>;

    const fakeEvent = (
        x: number,
        y: number,
        button: number,
    ): React.MouseEvent =>
        ({
            clientX: x,
            clientY: y,
            button,
        }) as unknown as React.MouseEvent;

    beforeEach(() => {
        store = createChessboardStore();
    });

    describe("subscribePointerDown / unsubscribePointerDown", () => {
        it("should add and remove pointerDown listeners", () => {
            const handler = vi.fn();
            const { subscribePointerDown, unsubscribePointerDown } =
                store.getState();

            subscribePointerDown(handler);
            expect(store.getState().onPointerDownListeners.has(handler)).toBe(
                true,
            );

            unsubscribePointerDown(handler);
            expect(store.getState().onPointerDownListeners.has(handler)).toBe(
                false,
            );
        });
    });

    describe("subscribePointerUp / unsubscribePointerUp", () => {
        it("should add and remove pointerUp listeners", () => {
            const handler = vi.fn();
            const { subscribePointerUp, unsubscribePointerUp } =
                store.getState();

            subscribePointerUp(handler);
            expect(store.getState().onPointerUpListeners.has(handler)).toBe(
                true,
            );

            unsubscribePointerUp(handler);
            expect(store.getState().onPointerUpListeners.has(handler)).toBe(
                false,
            );
        });
    });

    describe("onPointerDown", () => {
        it("should set interaction state", () => {
            const slice = store.getState();
            const event = fakeEvent(50, 100, 1);

            slice.onPointerDown(event);

            expect(store.getState().interaction).toEqual<InteractionInfo>({
                point: { x: 50, y: 100 },
                button: 1,
            });
        });

        it("should notify subscribed pointerDown listeners", () => {
            const handler = vi.fn();
            const slice = store.getState();
            const event = fakeEvent(10, 20, 0);

            slice.subscribePointerDown(handler);
            slice.onPointerDown(event);

            expect(handler).toHaveBeenCalledWith({
                point: { x: 10, y: 20 },
                button: 0,
            });
        });
    });

    describe("onPointerUp", () => {
        it("should clear interaction state", () => {
            const slice = store.getState();
            const downEvent = fakeEvent(0, 0, 0);
            const upEvent = fakeEvent(100, 200, 2);

            // simulate setting interaction first
            slice.onPointerDown(downEvent);
            expect(store.getState().interaction).not.toBe(null);

            slice.onPointerUp(upEvent);
            expect(store.getState().interaction).toBe(null);
        });

        it("should notify subscribed pointerUp listeners", () => {
            const handler = vi.fn();
            const slice = store.getState();
            const event = fakeEvent(75, 125, 2);

            slice.subscribePointerUp(handler);
            slice.onPointerUp(event);

            expect(handler).toHaveBeenCalledWith({
                point: { x: 75, y: 125 },
                button: 2,
            });
        });
    });
});
