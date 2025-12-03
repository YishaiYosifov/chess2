import { render, screen } from "@testing-library/react";
import React, { act } from "react";

import OngoingGamesPopup from "../OngoingGamesPopup";
import { PopupRef } from "@/components/Popup";
import useLobbyStore from "../../stores/lobbyStore";
import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";

describe("OngoingGamesPopup", () => {
    const ref = React.createRef<PopupRef>();

    it("should not render popup content by default", async () => {
        render(<OngoingGamesPopup ref={ref} />);

        expect(
            screen.queryByTestId("ongoingGamesPopup"),
        ).not.toBeInTheDocument();
    });

    it("should open the popup when open is called", async () => {
        render(<OngoingGamesPopup ref={ref} />);
        act(() => ref.current?.open());

        expect(screen.getByTestId("ongoingGamesPopup")).toBeInTheDocument();
        expect(screen.getByText("Ongoing Games")).toBeInTheDocument();
    });

    it("should render each ongoing game item", () => {
        const games = [
            createFakeOngoingGame(),
            createFakeOngoingGame(),
            createFakeOngoingGame(),
        ];
        useLobbyStore.getState().addOngoingGames(games);

        render(<OngoingGamesPopup ref={ref} />);
        act(() => ref.current?.open());

        for (const game of games) {
            expect(
                screen.getByTestId(`ongoingGameItem-${game.gameToken}`),
            ).toBeInTheDocument();
        }
    });
});
