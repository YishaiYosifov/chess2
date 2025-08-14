import { render, screen } from "@testing-library/react";
import { act } from "react";

import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import OpenSeekDirectory from "../OpenSeekDirectory";
import {
    OpenSeekClientEvents,
    useOpenSeekEmitter,
    useOpenSeekEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekerFaker";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("OpenSeekDirectory", () => {
    const openSeekHandlers: EventHandlers<OpenSeekClientEvents> = {};

    const sendOpenSeekEvent = vi.fn();
    const useOpenSeekEventMock = vi.mocked(useOpenSeekEvent);

    beforeEach(() => {
        vi.mocked(useOpenSeekEmitter).mockReturnValue(sendOpenSeekEvent);
        useOpenSeekEventMock.mockImplementation((event, handler) => {
            openSeekHandlers[event] = handler;
        });

        vi.useFakeTimers();
    });

    it("should subscribe to open seek events on mount", () => {
        render(<OpenSeekDirectory />);
        expect(sendOpenSeekEvent).toHaveBeenCalledWith("SubscribeAsync");
    });

    it("should display new open seeks when received", async () => {
        render(<OpenSeekDirectory />);

        const newSeek = createFakeOpenSeek({});
        act(() => openSeekHandlers["NewOpenSeeksAsync"]?.([newSeek]));

        expect(screen.getByTestId("openSeekUsername")).toHaveTextContent(
            newSeek.userName,
        );
    });

    it("should remove open seek when OpenSeekEndedAsync is received", async () => {
        render(<OpenSeekDirectory />);

        const seek = createFakeOpenSeek();

        act(() => openSeekHandlers["NewOpenSeeksAsync"]?.([seek]));
        act(() =>
            openSeekHandlers["OpenSeekEndedAsync"]?.(seek.userId, seek.pool),
        );
        act(() => vi.advanceTimersByTime(500));

        expect(screen.queryByTestId("openSeek")).not.toBeInTheDocument();
    });

    it("should limit open seeks to 10", async () => {
        render(<OpenSeekDirectory />);

        const seeks = Array.from({ length: 15 }, () => createFakeOpenSeek());

        act(() => openSeekHandlers["NewOpenSeeksAsync"]?.(seeks));

        const items = screen.getAllByTestId("openSeek");
        expect(items.length).toBe(10);
    });
});
