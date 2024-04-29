import useWebSocket, {
    Options as useWebsocketOptions,
} from "react-use-websocket";
import { Mock } from "vitest";

import { useSharedWSSilent, useSharedWSEvent, WSEvent } from "../useSharedWS";

vi.mock("react-use-websocket");
const useWebSocketMock = useWebSocket as Mock;

describe("useSharedWSSilent", () => {
    it("should provide the correct options", () => {
        useSharedWSSilent();
        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];

        const closeEvent = new CloseEvent("");
        const messageEvent = new MessageEvent("");

        expect(options.shouldReconnect?.(closeEvent)).toBeTruthy();
        expect(options.filter?.(messageEvent)).toBeFalsy();
        expect(options.share).toBeTruthy();
    });

    it("should pass custom options", () => {
        const customOptions = { protocols: "test" };
        useSharedWSSilent(customOptions);

        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];
        expect(options).toEqual(expect.objectContaining(customOptions));
    });
});

describe("useSharedWSEvent", () => {
    it("should provide the correct options", () => {
        useSharedWSEvent(WSEvent.GameStart);
        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];

        // make sure it filters out bad events
        const badEventMessage = new MessageEvent("message", {
            data: `${WSEvent.Notification}:"test"`,
        });
        expect(options.filter?.(badEventMessage)).toBeFalsy();

        const goodEventMessage = new MessageEvent("message", {
            data: `${WSEvent.GameStart}:"test"`,
        });
        expect(options.filter?.(goodEventMessage)).toBeTruthy();
    });

    it("should return parse and return the message when received", () => {
        const data = { test: "ing" };
        useWebSocketMock.mockReturnValue({
            lastMessage: {
                data: `${WSEvent.GameStart}:${JSON.stringify(data)}`,
            },
        });

        expect(useSharedWSEvent(WSEvent.GameStart).data).toEqual(data);
    });

    it("should return null when lastMessage is not present", () => {
        expect(useSharedWSEvent(WSEvent.GameStart).data).toBeNull();
    });
});
