import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor, GameResult } from "@/lib/apiClient";
import { act, render, screen } from "@testing-library/react";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import GameClock from "../GameClock";

describe("GameClock", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        vi.useFakeTimers();
        vi.setSystemTime(1000);
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({
                clocks: {
                    whiteClock: 300000,
                    blackClock: 300000,
                    lastUpdated: Date.now().valueOf(),
                },
                sideToMove: GameColor.WHITE,
            }),
        );
    });

    it("should render initial time correctly", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should count down over time when active", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10000); // 10 seconds
        });

        expect(screen.getByText("04:50")).toBeInTheDocument();
    });

    it("should not count down when it's not the player's turn", () => {
        store.setState({ sideToMove: GameColor.BLACK });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10000);
        });

        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should freeze clock when game is over", () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "description",
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10000);
        });

        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should show decimal seconds and animate under 20s", () => {
        store.setState({
            clocks: {
                whiteClock: 15000,
                blackClock: 300000,
                lastUpdated: Date.now().valueOf(),
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(5000);
        });

        const clock = screen.getByText(/00:10\.\d\d/);
        expect(clock).toBeInTheDocument();
        expect(clock.classList.contains("animate-freakout")).toBe(true);
    });

    it("should apply 'text-red-600' class when clock is zero and game is over", () => {
        store.setState({
            clocks: {
                whiteClock: 0,
                blackClock: 300000,
                lastUpdated: 0,
            },
            sideToMove: GameColor.WHITE,
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "timeout",
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(1000);
        });

        const clock = screen.getByText("00:00.00");
        expect(clock.classList.contains("text-red-600")).toBe(true);
        expect(clock.classList.contains("animate-freakout")).toBe(false);
    });

    it("should show zero and doesn't go negative", () => {
        store.setState({
            clocks: {
                whiteClock: 5000,
                blackClock: 300000,
                lastUpdated: Date.now().valueOf(),
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(7000);
        });

        expect(screen.getByText("00:00.00")).toBeInTheDocument();
    });

    it("should stop ticking when lastUpdated is null", () => {
        store.setState({
            clocks: {
                whiteClock: 1000,
                blackClock: 1000,
                lastUpdated: null,
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(1000);
        });

        expect(screen.getByText("00:01.00")).toBeInTheDocument();
    });
});
