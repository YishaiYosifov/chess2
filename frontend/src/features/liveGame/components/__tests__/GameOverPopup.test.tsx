import { act, render, screen } from "@testing-library/react";
import React from "react";

import { GameColor, GamePlayer } from "@/lib/apiClient";
import useLiveChessboardStore, {
    GameResultData,
} from "@/features/liveGame/stores/liveChessboardStore";
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
    const result: GameResultData = {
        result: GameResult.WHITE_WIN,
        resultDescription: "White Won by Resignation",
        whiteRatingDelta: +10,
        blackRatingDelta: -8,
    };
    const ref = React.createRef<GameOverPopupRef>();

    beforeEach(() => {
        useLiveChessboardStore.setState({
            whitePlayer,
            blackPlayer,
            resultData: result,
        });
    });

    it("should not render by default", () => {
        render(<GameOverPopup ref={ref} />);
        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should render when opened via ref", () => {
        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByTestId("gameOverPopup")).toBeInTheDocument();
        expect(screen.getByText(/game over/i)).toBeInTheDocument();
        expect(screen.getByText("White")).toBeInTheDocument();
        expect(screen.getByText("Black")).toBeInTheDocument();
        expect(screen.getByText("+10")).toBeInTheDocument();
        expect(screen.getByText("-8")).toBeInTheDocument();

        const whiteCard = screen.getByTestId("gameOverPopupProfile-0");
        const blackCard = screen.getByTestId("gameOverPopupProfile-1");

        expect(whiteCard.className).toMatch(/border-amber-500/);
        expect(blackCard.className).not.toMatch(/border-amber-500/);
    });

    it("should close when clicking background", async () => {
        const user = userEvent.setup();

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        const background = screen.getByTestId("gameOverPopupBackground");
        await user.click(background);

        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should close when clicking × button", async () => {
        const user = userEvent.setup();

        render(<GameOverPopup ref={ref} />);

        act(() => ref.current?.open());
        await user.click(screen.getByTestId("closeGameOverPopup"));

        expect(screen.queryByTestId("gameOverPopup")).not.toBeInTheDocument();
    });

    it("should not close when clicking on the popup, instead of the background", async () => {
        const user = userEvent.setup();

        render(<GameOverPopup ref={ref} />);
        act(() => ref.current?.open());

        const popup = screen.getByTestId("gameOverPopup");
        await user.click(popup);

        expect(screen.queryByTestId("gameOverPopup")).toBeInTheDocument();
    });
});
