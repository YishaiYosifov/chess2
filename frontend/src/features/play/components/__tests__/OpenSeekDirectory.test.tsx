import { render, screen } from "@testing-library/react";
import { act } from "react";

import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import OpenSeekDirectory from "../OpenSeekDirectory";
import createFakeOpenSeek from "@/lib/testUtils/fakers/openSeekerFaker";
import constants from "@/lib/constants";
import {
    OpenSeekClientEvents,
    useOpenSeekEmitter,
    useOpenSeekEvent,
} from "@/features/lobby/hooks/useOpenSeekHub";

vi.mock("@/features/lobby/hooks/useOpenSeekHub");
vi.mock("@/features/lobby/hooks/useLobbyHub");

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

    it("should display new open seeks when received", () => {
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
        await act(() => vi.advanceTimersByTimeAsync(500));

        expect(screen.queryByTestId("openSeek")).not.toBeInTheDocument();
    });

    it("should display 'No open challenges' text", async () => {
        render(<OpenSeekDirectory />);

        expect(screen.getByTestId("noOpenChallengesText")).toHaveTextContent(
            "No open challenges, join a pool to appear here for others",
        );
    });

    it("should hide no open challenges text when new seeks arrive", async () => {
        render(<OpenSeekDirectory />);

        const newSeek = createFakeOpenSeek({});
        act(() => openSeekHandlers["NewOpenSeeksAsync"]?.([newSeek]));
        await act(() => vi.advanceTimersToNextFrame());

        expect(
            screen.queryByTestId("noOpenChallengesText"),
        ).not.toBeInTheDocument();
    });

    it("should wait before showing no open challenges text after seeks disappear", async () => {
        render(<OpenSeekDirectory />);

        const newSeek = createFakeOpenSeek({});
        act(() => openSeekHandlers["NewOpenSeeksAsync"]?.([newSeek]));
        await act(() => vi.advanceTimersToNextFrame());

        act(() =>
            openSeekHandlers["OpenSeekEndedAsync"]?.(
                newSeek.userId,
                newSeek.pool,
            ),
        );
        await act(() => vi.advanceTimersToNextFrame());
        expect(
            screen.queryByTestId("noOpenChallengesText"),
        ).not.toBeInTheDocument();

        await act(() => vi.advanceTimersByTimeAsync(300));
        expect(screen.getByTestId("noOpenChallengesText")).toBeInTheDocument();
    });

    it("should repeatedly send SubscribeAsync at interval", () => {
        render(<OpenSeekDirectory />);

        expect(sendOpenSeekEvent).toHaveBeenCalledTimes(1);
        expect(sendOpenSeekEvent).toHaveBeenCalledWith("SubscribeAsync");

        act(() => {
            vi.advanceTimersByTime(
                constants.OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
            );
        });
        expect(sendOpenSeekEvent).toHaveBeenCalledTimes(2);

        act(() => {
            vi.advanceTimersByTime(
                2 * constants.OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
            );
        });
        expect(sendOpenSeekEvent).toHaveBeenCalledTimes(4);
    });
});
