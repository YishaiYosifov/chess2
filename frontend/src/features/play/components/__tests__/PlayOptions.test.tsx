import { render, screen } from "@testing-library/react";

import PlayOptions from "../PlayOptions";
import constants from "@/lib/constants";
import React from "react";
import userEvent from "@testing-library/user-event";
import { mockJsCookie } from "@/lib/testUtils/mocks/mockCookies";
import { PoolType } from "@/lib/apiClient";

vi.mock("js-cookie");
vi.mock("@/features/signalR/hooks/useSignalRHubs");

describe("PlayOptions", () => {
    function mockIsAuthedCookie(isAuthed: boolean) {
        const cookieValue = isAuthed ? "true" : undefined;
        mockJsCookie({ [constants.COOKIES.IS_LOGGED_IN]: cookieValue });
    }

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

    it("should render casual PoolButtons when isRated is false", () => {
        render(<PlayOptions />);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).not.toBeVisible();
    });

    it("should render rated PoolButtons when isRated is true", async () => {
        const user = userEvent.setup();
        mockIsAuthedCookie(true);
        render(<PlayOptions />);
        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });

    it("should persist isRated across mounts", async () => {
        const user = userEvent.setup();
        mockIsAuthedCookie(true);

        const { unmount } = render(<PlayOptions />);
        const poolToggle = screen.getByTestId("poolToggle");

        await user.click(poolToggle);
        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();

        unmount();

        render(<PlayOptions />);

        expect(
            screen.getByTestId(`poolButtonsSection-${PoolType.RATED}`),
        ).toBeVisible();
        expect(
            screen.queryByTestId(`poolButtonsSection-${PoolType.CASUAL}`),
        ).not.toBeVisible();
    });
});
