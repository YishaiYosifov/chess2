import { render, screen } from "@testing-library/react";

import PlayOptions from "../PlayOptions";
import constants from "@/lib/constants";
import {
    useMatchmakingEmitter,
    useMatchmakingEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import React, { act } from "react";
import userEvent from "@testing-library/user-event";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";

vi.mock("js-cookie");
vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("PlayOptions", () => {
    const sendMatchmakingEventMock = vi.fn();
    let matchFoundCallback: ((token: string) => void) | undefined;
    let matchFailedCallback: (() => void) | undefined;

    function mockIsAuthedCookie(isAuthed: boolean) {
        const cookieValue = isAuthed ? "true" : undefined;
        mockJsCookie({
            [constants.COOKIES.IS_AUTHED]: cookieValue,
        });
    }

    beforeEach(() => {
        vi.mocked(useMatchmakingEvent).mockImplementation(
            (eventName, callback) => {
                if (eventName === "MatchFoundAsync")
                    matchFoundCallback = callback;
                else if (eventName === "MatchFailedAsync")
                    matchFailedCallback = callback;
            },
        );

        vi.mocked(useMatchmakingEmitter).mockReturnValue(
            sendMatchmakingEventMock,
        );
    });

    it("should render the heading and main container", () => {
        render(<PlayOptions />);

        expect(screen.getByText("Play Chess 2")).toBeInTheDocument();
        expect(screen.getByTestId("playOptions")).toBeInTheDocument();
    });

    it("should show PoolToggle when authenticated", () => {
        mockIsAuthedCookie(true);

        render(<PlayOptions />);

        expect(screen.getByTestId("poolToggle")).toBeInTheDocument();
    });

    it("should hide PoolToggle when unauthenticated", () => {
        mockIsAuthedCookie(false);

        render(<PlayOptions />);

        expect(screen.queryByTestId("poolToggle")).not.toBeInTheDocument();
    });

    it("should navigate to game route when MatchFoundAsync is triggered", () => {
        const testToken = "test-token";
        const routerMock = mockRouter();

        render(<PlayOptions />);

        act(() => matchFoundCallback?.(testToken));

        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${testToken}`,
        );
    });

    it("should stop seeking when MatchFailedAsync is triggered", async () => {
        const user = userEvent.setup();
        mockIsAuthedCookie(true);

        render(<PlayOptions />);

        const timeBtn = screen.getByText(
            `${constants.STANDARD_TIME_CONTROLS[0].settings.baseSeconds / 60} + ${constants.STANDARD_TIME_CONTROLS[0].settings.incrementSeconds}`,
        );
        await user.click(timeBtn);

        expect(screen.getByTestId("seekingOverlay")).toBeInTheDocument();
        act(() => matchFailedCallback?.());
        expect(screen.queryByTestId("seekingOverlay")).not.toBeInTheDocument();
    });

    it("should call SeekRatedAsync when isRated is true", async () => {
        const user = userEvent.setup();
        mockIsAuthedCookie(true);

        render(<PlayOptions />);
        const playButton = screen.getByText(
            `${constants.STANDARD_TIME_CONTROLS[0].settings.baseSeconds / 60} + ${constants.STANDARD_TIME_CONTROLS[0].settings.incrementSeconds}`,
        );

        const poolToggle = screen.getByTestId("poolToggle");

        // toggle to rated
        await user.click(poolToggle);

        await user.click(playButton);

        expect(sendMatchmakingEventMock).toHaveBeenCalledWith(
            "SeekRatedAsync",
            constants.STANDARD_TIME_CONTROLS[0].settings,
        );
    });

    it("should call CancelSeekAsync when SeekingOverlay is clicked", async () => {
        const user = userEvent.setup();

        render(<PlayOptions />);

        const timeBtn = screen.getByText(
            `${constants.STANDARD_TIME_CONTROLS[0].settings.baseSeconds / 60} + ${constants.STANDARD_TIME_CONTROLS[0].settings.incrementSeconds}`,
        );
        await user.click(timeBtn);

        const cancelSeekButton = screen.getByTestId("cancelSeekButton");
        await user.click(cancelSeekButton);

        expect(sendMatchmakingEventMock).toHaveBeenLastCalledWith(
            "CancelSeekAsync",
        );
    });
});
