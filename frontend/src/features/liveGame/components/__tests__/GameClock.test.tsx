import { createFakeLiveChessStore } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../../stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { act, render, screen } from "@testing-library/react";
import LiveChessStoreContext from "../../contexts/liveChessContext";
import GameClock from "../GameClock";
import { GameResult } from "@/types/tempModels";

describe("GameClock", () => {
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createFakeLiveChessStore({
            clocks: {
                whiteClock: 300000,
                blackClock: 300000,
                lastUpdated: 0,
            },
            sideToMove: GameColor.WHITE,
        });
        vi.useFakeTimers();
        vi.setSystemTime(0);
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

    it("should show decimal seconds under 30s", () => {
        store.setState({
            clocks: {
                whiteClock: 25000, // 25 seconds
                blackClock: 300000,
                lastUpdated: 0,
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(5000); // 5 seconds pass
        });

        // Should display decimal format: "00:20.xx"
        const text = screen.getByText(/00:20\.\d\d/);
        expect(text).toBeInTheDocument();
    });

    it("should show zero and doesn't go negative", () => {
        store.setState({
            clocks: {
                whiteClock: 5000,
                blackClock: 300000,
                lastUpdated: 0,
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
});
