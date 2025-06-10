import { render, screen } from "@testing-library/react";

import PlayOptions from "../PlayOptions";
import constants from "@/lib/constants";
import {
    useMatchmakingEmitter,
    useMatchmakingEvent,
} from "@/hooks/signalR/useSignalRHubs";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import React from "react";
import userEvent from "@testing-library/user-event";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";

vi.mock("js-cookie");
vi.mock("@/hooks/signalR/useSignalRHubs");

describe("PlayOptions", () => {
    const sendMatchmakingEventMock = vi.fn();
    let matchFoundCallback: (token: string) => void;

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

        matchFoundCallback(testToken);

        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${testToken}`,
        );
    });

    it("should call SeekRatedAsync when isRated is true", async () => {
        const user = userEvent.setup();
        mockIsAuthedCookie(true);

        render(<PlayOptions />);
        const playButton = screen.getByText(
            `${constants.TIME_CONTROLS[0].baseMinutes} + ${constants.TIME_CONTROLS[0].increment}`,
        );

        const poolToggle = screen.getByTestId("poolToggle");

        // toggle to rated
        await user.click(poolToggle);

        await user.click(playButton);

        expect(sendMatchmakingEventMock).toHaveBeenCalledWith(
            "SeekRatedAsync",
            constants.TIME_CONTROLS[0].baseMinutes,
            constants.TIME_CONTROLS[0].increment,
        );
    });

    it("should call CancelSeekAsync when SeekingOverlay is clicked", async () => {
        const user = userEvent.setup();

        render(<PlayOptions />);

        const timeBtn = screen.getByText(
            `${constants.TIME_CONTROLS[0].baseMinutes} + ${constants.TIME_CONTROLS[0].increment}`,
        );
        await user.click(timeBtn);

        const cancelSeekButton = screen.getByTestId("cancelSeekButton");
        await user.click(cancelSeekButton);

        expect(sendMatchmakingEventMock).toHaveBeenLastCalledWith(
            "CancelSeekAsync",
        );
    });
});
