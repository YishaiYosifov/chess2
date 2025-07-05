import { act, render, screen } from "@testing-library/react";
import React from "react";

import { LiveChessStore } from "@/features/liveGame/stores/liveChessboardStore";
import { GameColor } from "@/lib/apiClient";
import { GameResult } from "@/types/tempModels";
import GameOverPopup, { GameOverPopupRef } from "../GameOverPopup";
import userEvent from "@testing-library/user-event";
import { createFakeLiveChessStore } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { LiveChessStoreContext } from "../../contexts/liveChessContext";
import { StoreApi } from "zustand";

describe("GameOverPopup", () => {
    const ref = React.createRef<GameOverPopupRef>();
    let store: StoreApi<LiveChessStore>;

    beforeEach(() => {
        store = createFakeLiveChessStore({ playerColor: GameColor.WHITE });
    });

    it("should not render popup content by default", () => {
        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should show victory title and rating deltas for white win", () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Checkmate",
                whiteRatingDelta: 12,
                blackRatingDelta: -10,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
        expect(screen.getByText("VICTORY")).toBeInTheDocument();
        expect(screen.getByText("White Won by Checkmate")).toBeInTheDocument();
        expect(screen.getByText("+12")).toBeInTheDocument();
        expect(screen.getByText("-10")).toBeInTheDocument();
    });

    it("should show 'YOU LOST' when black wins and player is white", () => {
        store.setState({
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "Black Won on Time",
                whiteRatingDelta: -15,
                blackRatingDelta: +18,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByText("YOU LOST")).toBeInTheDocument();
        expect(screen.getByText("Black Won on Time")).toBeInTheDocument();
    });

    it("should show DRAW title and result description", () => {
        store.setState({
            resultData: {
                result: GameResult.DRAW,
                resultDescription: "Draw by Stalemate",
                whiteRatingDelta: 0,
                blackRatingDelta: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByText("DRAW")).toBeInTheDocument();
        expect(screen.getByText("Draw by Stalemate")).toBeInTheDocument();
    });

    it("should show ABORTED title if game was aborted", () => {
        store.setState({
            resultData: {
                result: GameResult.ABORTED,
                resultDescription: "Game Aborted",
                whiteRatingDelta: 0,
                blackRatingDelta: 0,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByText("ABORTED")).toBeInTheDocument();
        expect(screen.getByText("Game Aborted")).toBeInTheDocument();
    });

    it("should close when clicking background", async () => {
        const user = userEvent.setup();
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("gameOverPopupBackground"));
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should close when clicking × button", async () => {
        const user = userEvent.setup();
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("closeGameOverPopup"));
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should not close when clicking on the popup content", async () => {
        const user = userEvent.setup();
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("gameOverPopup"));
        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
    });

    it("should render NEW GAME and REMATCH buttons", () => {
        store.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(
            <LiveChessStoreContext.Provider value={store}>
                <GameOverPopup ref={ref} />
            </LiveChessStoreContext.Provider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByText("NEW GAME")).toBeInTheDocument();
        expect(screen.getByText("REMATCH")).toBeInTheDocument();
    });
});
