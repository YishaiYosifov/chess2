import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor } from "@/lib/apiClient";
import { act, render, screen } from "@testing-library/react";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import GameClock from "../GameClock";

vi.mock("@/features/audio/audioPlayer");

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
                    isFrozen: false,
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

    it("should freeze clock when isFrozen is true", () => {
        store.setState({
            clocks: {
                whiteClock: 300000,
                blackClock: 300000,
                lastUpdated: Date.now(),
                isFrozen: true,
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        act(() => {
            vi.advanceTimersByTime(10000);
        });

        // should still show initial time because frozen
        expect(screen.getByText("05:00")).toBeInTheDocument();
    });

    it("should show decimal seconds and animate under 20s", () => {
        store.setState({
            clocks: {
                whiteClock: 15000,
                blackClock: 300000,
                lastUpdated: Date.now().valueOf(),
                isFrozen: false,
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

    it("should apply 'text-red-600' class when clock is zero and frozen", () => {
        store.setState({
            clocks: {
                whiteClock: 0,
                blackClock: 300000,
                lastUpdated: Date.now(),
                isFrozen: true,
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
                isFrozen: false,
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

    it("should stop ticking when isFrozen is true", () => {
        store.setState({
            clocks: {
                whiteClock: 1000,
                blackClock: 1000,
                lastUpdated: Date.now().valueOf(),

                isFrozen: true,
            },
            sideToMove: GameColor.WHITE,
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => {
            vi.advanceTimersByTime(10000);
        });

        expect(screen.getByText("00:01.00")).toBeInTheDocument();
    });

    it("should play warning sound once when time goes under 20s", () => {
        store.setState({
            clocks: {
                whiteClock: 21000,
                blackClock: 300000,
                lastUpdated: Date.now(),
                isFrozen: false,
            },
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();

        act(() => {
            vi.advanceTimersByTime(2000);
        });

        expect(AudioPlayer.playAudio).toHaveBeenCalledExactlyOnceWith(
            AudioType.LOW_TIME,
        );
    });

    it("should not play sound if viewer is not the player", () => {
        store.setState({
            clocks: {
                whiteClock: 15000,
                blackClock: 300000,
                lastUpdated: Date.now(),
                isFrozen: false,
            },
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.BLACK, userId: "id" }, // not same color
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });

    it("should not play sound when clock is frozen", () => {
        store.setState({
            clocks: {
                whiteClock: 15000,
                blackClock: 300000,
                lastUpdated: Date.now(),
                isFrozen: true,
            },
            sideToMove: GameColor.WHITE,
            viewer: { playerColor: GameColor.WHITE, userId: "id" },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameClock color={GameColor.WHITE} />
            </LiveChessStoreContext.Provider>,
        );

        expect(AudioPlayer.playAudio).not.toHaveBeenCalled();
    });
});
