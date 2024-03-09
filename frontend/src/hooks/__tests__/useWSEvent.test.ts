import useWSEvent, { WSEvent } from "../useWSEvent";
import useWebSocket, {
    Options as useWebsocketOptions,
} from "react-use-websocket";
import { Mock } from "vitest";

vi.mock("react-use-websocket");

describe("useWSEvent", () => {
    const useWebSocketMock = useWebSocket as Mock;

    it("should provide the correct props", () => {
        useWSEvent(WSEvent.GameStart);

        const options: useWebsocketOptions = useWebSocketMock.mock.calls[0][1];
        expect(options.shouldReconnect?.(new CloseEvent(""))).toBeTruthy();
        expect(options.share).toBeTruthy();

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

        expect(useWSEvent(WSEvent.GameStart)).toEqual(data);
    });

    it("should return null when lastMessage is not present", () => {
        expect(useWSEvent(WSEvent.GameStart)).toBeNull();
    });
});
