import { act, render, screen } from "@testing-library/react";
import React from "react";

import createLiveChessStore, {
    LiveChessStore,
} from "@/features/liveGame/stores/liveChessStore";
import { GameColor, GameResult } from "@/lib/apiClient";
import GameOverPopup, { GameOverPopupRef } from "../GameOverPopup";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import LiveChessStoreContext from "@/features/liveGame/contexts/liveChessContext";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { StoreApi } from "zustand";

vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("GameOverPopup", () => {
    const ref = React.createRef<GameOverPopupRef>();
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createLiveChessStore(
            createFakeLiveChessStoreProps({ viewerColor: GameColor.WHITE }),
        );
    });

    it("should not render popup content by default", async () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        expect(screen.queryByTestId("popup")).not.toBeInTheDocument();
    });

    it("should show victory title and rating changes for white win", async () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Checkmate",
                whiteRatingChange: 12,
                blackRatingChange: -10,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        act(() => ref.current?.open());

        expect(screen.getByTestId("popup")).toBeInTheDocument();
        expect(screen.getByText("VICTORY")).toBeInTheDocument();
        expect(screen.getByText("White Won by Checkmate")).toBeInTheDocument();
        expect(screen.getByText("+12")).toBeInTheDocument();
        expect(screen.getByText("-10")).toBeInTheDocument();
    });

    it("should show 'YOU LOST' when black wins and player is white", async () => {
        store.setState({
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "Black Won on Time",
                whiteRatingChange: -15,
                blackRatingChange: +18,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        act(() => ref.current?.open());

        expect(screen.getByText("YOU LOST")).toBeInTheDocument();
        expect(screen.getByText("Black Won on Time")).toBeInTheDocument();
    });

    it("should show DRAW title and result description", async () => {
        store.setState({
            resultData: {
                result: GameResult.DRAW,
                resultDescription: "Draw by Stalemate",
                whiteRatingChange: 0,
                blackRatingChange: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        act(() => ref.current?.open());

        expect(screen.getByText("DRAW")).toBeInTheDocument();
        expect(screen.getByText("Draw by Stalemate")).toBeInTheDocument();
    });

    it("should show ABORTED title if game was aborted", async () => {
        store.setState({
            resultData: {
                result: GameResult.ABORTED,
                resultDescription: "Game Aborted",
                whiteRatingChange: 0,
                blackRatingChange: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        act(() => ref.current?.open());

        expect(screen.getByText("ABORTED")).toBeInTheDocument();
        expect(screen.getByText("Game Aborted")).toBeInTheDocument();
    });

    it("should render NEW GAME and REMATCH buttons", async () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingChange: 10,
                blackRatingChange: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        await flushMicrotasks();
        act(() => ref.current?.open());

        expect(screen.getByText("NEW GAME")).toBeInTheDocument();
        expect(screen.getByText("REMATCH")).toBeInTheDocument();
    });
});
