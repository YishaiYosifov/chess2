import useWebSocket, {
    Options as useWebsocketOptions,
} from "react-use-websocket";
import { Mock } from "vitest";

import { useEventWebSocket } from "../useEventWS";
import { WSEventIn } from "@/lib/models";
import { renderHook } from "@testing-library/react";

vi.mock("react-use-websocket");

describe("useEventWebSocket", () => {
    const useWebSocketMock = useWebSocket as Mock;

    it("should provide the correct options", () => {
        renderHook(() => useEventWebSocket(WSEventIn.GameStart));
        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];

        const closeEvent = new CloseEvent("");
        const messageEvent = new MessageEvent("");

        expect(options.shouldReconnect?.(closeEvent)).toBeTruthy();
        expect(options.filter?.(messageEvent)).toBeFalsy();
        expect(options.share).toBeTruthy();

        // make sure it filters out bad events
        const badEventMessage = new MessageEvent("message", {
            data: `${WSEventIn.Notification}:"test"`,
        });
        const goodEventMessage = new MessageEvent("message", {
            data: `${WSEventIn.GameStart}:"test"`,
        });

        expect(options.filter?.(badEventMessage)).toBeFalsy();
        expect(options.filter?.(goodEventMessage)).toBeTruthy();
    });

    it("should pass custom options", () => {
        const customOptions = { protocols: "test" };
        renderHook(() => useEventWebSocket(undefined, customOptions));

        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];
        expect(options).toEqual(expect.objectContaining(customOptions));
    });

    it("should return parse and return the message when received", () => {
        const data = { test: "ing" };
        useWebSocketMock.mockReturnValue({
            lastMessage: {
                data: `${WSEventIn.GameStart}:${JSON.stringify(data)}`,
            },
        });

        const { result } = renderHook(() =>
            useEventWebSocket(WSEventIn.GameStart)
        );
        expect(result.current.lastData).toEqual(data);
    });

    it("should return null when lastMessage is not present", () => {
        const { result } = renderHook(() =>
            useEventWebSocket(WSEventIn.GameStart)
        );
        expect(result.current.lastData).toBeNull();
    });
});
