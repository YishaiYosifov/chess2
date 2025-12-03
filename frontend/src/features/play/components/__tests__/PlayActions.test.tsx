import { render, screen } from "@testing-library/react";

import userEvent from "@testing-library/user-event";
import PlayActions from "../PlayActions";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { PrivateUser } from "@/lib/apiClient";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";

describe("PlayActions", () => {
    let loggedInUserMock: PrivateUser;

    beforeEach(() => {
        loggedInUserMock = createFakePrivateUser();
    });

    it("should open challenge popup when clicking on challenge a friend", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayActions />
            </SessionProvider>,
        );

        await user.click(screen.getByText("Challenge a Friend"));

        expect(screen.getByTestId("challengePopup")).toBeInTheDocument();
    });

    it("should not display ongoing games button when there are no ongoing games", () => {
        useLobbyStore.setState({ ongoingGames: new Map() });

        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayActions />
            </SessionProvider>,
        );

        expect(
            screen.queryByText("Resume Ongoing Games"),
        ).not.toBeInTheDocument();
    });

    it("should open ongoing games popup when clicking on resumt ongoing games", async () => {
        useLobbyStore.setState({
            ongoingGames: new Map([
                ["token", createFakeOngoingGame({ gameToken: "token" })],
            ]),
        });

        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayActions />
            </SessionProvider>,
        );

        await user.click(screen.getByText("Resume Ongoing Games"));

        expect(screen.getByTestId("ongoingGamesPopup")).toBeInTheDocument();
    });
});
