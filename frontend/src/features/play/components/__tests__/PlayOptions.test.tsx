import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";

import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { GuestUser, PoolType, PrivateUser } from "@/lib/apiClient";
import useLobbyStore from "@/features/lobby/stores/lobbyStore";
import PlayOptions from "../PlayOptions";

vi.mock("@/features/lobby/hooks/useLobbyHub");

describe("PlayOptions", () => {
    let loggedInUserMock: PrivateUser;
    let guestUserMock: GuestUser;

    beforeEach(() => {
        loggedInUserMock = createFakePrivateUser();
        guestUserMock = createFakeGuestUser();
        useLobbyStore.setState(useLobbyStore.getInitialState());
    });

    it("should render the heading and main container", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(screen.getByText("Play Anarchy Chess")).toBeInTheDocument();
        expect(screen.getByTestId("playOptions")).toBeInTheDocument();
    });

    it("should show PoolToggle when authenticated", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(screen.getByTestId("poolToggle")).toBeInTheDocument();
    });

    it("should hide PoolToggle when unauthenticated", () => {
        render(
            <SessionProvider user={guestUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(screen.queryByTestId("poolToggle")).not.toBeInTheDocument();
    });

    it("should render casual PoolButtons when isRated is false", () => {
        render(
            <SessionProvider user={null} fetchAttempted>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).not.toBeVisible();
    });

    it("should render rated PoolButtons when isRated is true", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });

    it("should persist pool type across mounts", async () => {
        const user = userEvent.setup();

        const { unmount } = render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );
        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);
        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();

        unmount();
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });

    it("should reset pool type to casual when logging out", async () => {
        const user = userEvent.setup();

        const { rerender } = render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        await user.click(screen.getByTestId("poolToggle"));

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();

        rerender(
            <SessionProvider user={guestUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).not.toBeVisible();
    });

    it("should open challenge popup when clicking on challenge a friend", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
            </SessionProvider>,
        );

        await user.click(screen.getByText("Challenge a Friend"));

        expect(screen.getByTestId("challengePopup")).toBeInTheDocument();
    });

    it("should not display ongoing games button when there are no ongoing games", () => {
        useLobbyStore.setState({ ongoingGames: new Map() });

        render(
            <SessionProvider user={loggedInUserMock}>
                <PlayOptions />
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
                <PlayOptions />
            </SessionProvider>,
        );

        await user.click(screen.getByText("Resume Ongoing Games"));

        expect(screen.getByTestId("ongoingGamesPopup")).toBeInTheDocument();
    });
});
