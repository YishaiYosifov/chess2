import { act, render, screen } from "@testing-library/react";
import React from "react";

import useLiveChessboardStore from "@/features/liveGame/stores/liveChessboardStore";
import { GameColor, GamePlayer } from "@/lib/apiClient";
import { GameResult } from "@/types/tempModels";
import GameOverPopup, { GameOverPopupRef } from "../GameOverPopup";
import userEvent from "@testing-library/user-event";

describe("GameOverPopup", () => {
    const whitePlayer: GamePlayer = {
        userId: "w123",
        color: GameColor.WHITE,
        userName: "White",
        rating: 1400,
    };
    const blackPlayer: GamePlayer = {
        userId: "b456",
        color: GameColor.BLACK,
        userName: "Black",
        rating: 1350,
    };
    const ref = React.createRef<GameOverPopupRef>();

    beforeEach(() => {
        useLiveChessboardStore.setState({
            whitePlayer,
            blackPlayer,
            playerColor: GameColor.WHITE,
        });
    });

    it("should not render popup content by default", () => {
        render(<GameOverPopup ref={ref} />);
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should show victory title and rating deltas for white win", () => {
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Checkmate",
                whiteRatingDelta: 12,
                blackRatingDelta: -10,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
        expect(screen.getByText("VICTORY")).toBeInTheDocument();
        expect(screen.getByText("White Won by Checkmate")).toBeInTheDocument();
        expect(screen.getByText("+12")).toBeInTheDocument();
        expect(screen.getByText("-10")).toBeInTheDocument();
    });

    it("should show 'YOU LOST' when black wins and player is white", () => {
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.BLACK_WIN,
                resultDescription: "Black Won on Time",
                whiteRatingDelta: -15,
                blackRatingDelta: +18,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByText("YOU LOST")).toBeInTheDocument();
        expect(screen.getByText("Black Won on Time")).toBeInTheDocument();
    });

    it("should show DRAW title and result description", () => {
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.DRAW,
                resultDescription: "Draw by Stalemate",
                whiteRatingDelta: 0,
                blackRatingDelta: 0,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByText("DRAW")).toBeInTheDocument();
        expect(screen.getByText("Draw by Stalemate")).toBeInTheDocument();
    });

    it("should show ABORTED title if game was aborted", () => {
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.ABORTED,
                resultDescription: "Game Aborted",
                whiteRatingDelta: 0,
                blackRatingDelta: 0,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByText("ABORTED")).toBeInTheDocument();
        expect(screen.getByText("Game Aborted")).toBeInTheDocument();
    });

    it("should close when clicking background", async () => {
        const user = userEvent.setup();
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("gameOverPopupBackground"));
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should close when clicking × button", async () => {
        const user = userEvent.setup();
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("closeGameOverPopup"));
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should not close when clicking on the popup content", async () => {
        const user = userEvent.setup();
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        await user.click(screen.getByTestId("gameOverPopup"));
        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
    });

    it("should render NEW GAME and REMATCH buttons", () => {
        useLiveChessboardStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "White Won by Resignation",
                whiteRatingDelta: 10,
                blackRatingDelta: -8,
            },
        });

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByText("NEW GAME")).toBeInTheDocument();
        expect(screen.getByText("REMATCH")).toBeInTheDocument();
    });
});
