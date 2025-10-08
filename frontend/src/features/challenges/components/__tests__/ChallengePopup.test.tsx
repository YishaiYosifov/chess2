import { fireEvent, within, render, screen } from "@testing-library/react";
import { act } from "react";
import ChallengePopup, { ChallengePopupRef } from "../ChallengePopup";
import {
    createChallenge,
    ErrorCode,
    GuestUser,
    PoolType,
    PrivateUser,
    PublicUser,
} from "@/lib/apiClient";
import React from "react";
import {
    createFakeGuestUser,
    createFakePrivateUser,
    createFakeUser,
} from "@/lib/testUtils/fakers/userFaker";
import userEvent from "@testing-library/user-event";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import constants from "@/lib/constants";
import SessionProvider from "@/features/auth/contexts/sessionContext";

vi.mock("@/lib/apiClient/definition");

describe("ChallengePopup", () => {
    const ref = React.createRef<ChallengePopupRef>();
    const createChallengeMock = vi.mocked(createChallenge);

    let userMock: PublicUser;
    let loggedInUserMock: PrivateUser;
    let guestUserMock: GuestUser;

    beforeEach(() => {
        userMock = createFakeUser();
        loggedInUserMock = createFakePrivateUser();
        guestUserMock = createFakeGuestUser();
    });

    it("should not render popup content by default", async () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        expect(screen.queryByTestId("challengePopup")).not.toBeInTheDocument();
    });

    it("should open the popup when open is called", async () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByTestId("challengePopup")).toBeInTheDocument();
        expect(screen.getByText("Create Challenge")).toBeInTheDocument();
        expect(
            screen.getByText(`Challenge ${userMock.userName}`),
        ).toBeInTheDocument();
    });

    it("should show default minutes, increment, and pool type", async () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByTestId("challengePopupMinutes")).toHaveValue(
            constants.DEFAULT_CHALLENGE_MINUTE_OPTION_IDX.toString(),
        );
        expect(screen.getByTestId("challengePopupIncrement")).toHaveValue(
            constants.DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX.toString(),
        );
        expect(screen.getByTestId("challengePopupPoolType")).toHaveAttribute(
            "data-selected",
            PoolType.RATED.toString(),
        );
    });

    it("should pass correct time control and poolType to createChallenge", async () => {
        const challengeMock = createFakeChallengeRequest();
        vi.mocked(createChallenge).mockResolvedValue({
            data: challengeMock,
            response: new Response(),
        });
        const routerMock = mockRouter();
        const user = userEvent.setup();

        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        const minutesSlider = screen.getByTestId<HTMLInputElement>(
            "challengePopupMinutes",
        );
        fireEvent.change(minutesSlider, { target: { value: "5" } });

        const incrementSlider = screen.getByTestId<HTMLInputElement>(
            "challengePopupIncrement",
        );
        fireEvent.change(incrementSlider, { target: { value: "1" } });

        const poolTypeDiv = screen.getByTestId("challengePopupPoolType");
        const casualButton = within(poolTypeDiv).getByTestId(
            `selector-${PoolType.CASUAL}`,
        );
        await user.click(casualButton);

        const challengeButton = screen.getByTestId("challengePopupCreate");
        await user.click(challengeButton);

        expect(createChallengeMock).toHaveBeenCalledExactlyOnceWith({
            query: { recipientId: userMock.userId },
            body: {
                poolType: PoolType.CASUAL,
                timeControl: {
                    baseSeconds: constants.CHALLENGE_MINUTES_OPTIONS[5] * 60,
                    incrementSeconds:
                        constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[1],
                },
            },
        });
        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.CHALLENGE}/${challengeMock.challengeId}`,
        );
    });

    it("should show error message if challenge fails", async () => {
        createChallengeMock.mockResolvedValue({
            error: {
                errors: [
                    {
                        errorCode: ErrorCode.CHALLENGE_RECIPIENT_NOT_ACCEPTING,
                        description: "test error description",
                        metadata: {},
                    },
                ],
            },
            data: undefined,
            response: new Response(),
        });
        const user = userEvent.setup();

        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        const button = screen.getByTestId("challengePopupCreate");
        await user.click(button);

        expect(screen.getByTestId("challengePopupError")).toHaveTextContent(
            "test error description",
        );
    });

    it("should close when requested", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        const closeButton = screen.getByTestId("closePopup");
        await user.click(closeButton);

        expect(screen.queryByTestId("challengePopup")).not.toBeInTheDocument();
    });

    it("should persist preferences across mounts", async () => {
        const user = userEvent.setup();

        const { unmount } = render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        fireEvent.change(
            screen.getByTestId<HTMLInputElement>("challengePopupMinutes"),
            {
                target: { value: "2" },
            },
        );
        fireEvent.change(
            screen.getByTestId<HTMLInputElement>("challengePopupIncrement"),
            {
                target: { value: "1" },
            },
        );
        const poolTypeSelector = screen.getByTestId("challengePopupPoolType");
        const casualButton = within(poolTypeSelector).getByTestId(
            `selector-${PoolType.CASUAL}`,
        );
        await user.click(casualButton);

        unmount();

        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        expect(screen.getByTestId("challengePopupMinutes")).toHaveValue("2");
        expect(screen.getByTestId("challengePopupIncrement")).toHaveValue("1");
        expect(screen.getByTestId("challengePopupPoolType")).toHaveAttribute(
            "data-selected",
            PoolType.CASUAL.toString(),
        );
    });

    it("should display the correct minutes and increment text", async () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        const minutesText = screen.getByTestId("challengePopupMinutesText");
        const incrementText = screen.getByTestId("challengePopupIncrementText");
        const minutesSlider = screen.getByTestId<HTMLInputElement>(
            "challengePopupMinutes",
        );
        const incrementSlider = screen.getByTestId<HTMLInputElement>(
            "challengePopupIncrement",
        );

        fireEvent.change(minutesSlider, { target: { value: "2" } });
        expect(minutesText).toHaveTextContent(
            `Minutes per side: ${constants.CHALLENGE_MINUTES_OPTIONS[2]}`,
        );

        fireEvent.change(incrementSlider, { target: { value: "1" } });
        expect(incrementText).toHaveTextContent(
            `Increment in seconds: ${constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[1]}`,
        );
    });

    it("should not show pool type selector for guest users", async () => {
        render(
            <SessionProvider user={guestUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        expect(
            screen.queryByTestId("challengePopupPoolType"),
        ).not.toBeInTheDocument();
    });

    it("should force poolType to CASUAL when guest sends a challenge", async () => {
        const challengeMock = createFakeChallengeRequest();
        createChallengeMock.mockResolvedValue({
            data: challengeMock,
            response: new Response(),
        });
        const routerMock = mockRouter();
        const user = userEvent.setup();

        render(
            <SessionProvider user={guestUserMock}>
                <ChallengePopup ref={ref} profile={userMock} />
            </SessionProvider>,
        );
        act(() => ref.current?.open());

        const challengeButton = screen.getByTestId("challengePopupCreate");
        await user.click(challengeButton);

        expect(createChallengeMock).toHaveBeenCalledExactlyOnceWith({
            query: { recipientId: userMock.userId },
            body: {
                poolType: PoolType.CASUAL,
                timeControl: {
                    baseSeconds:
                        constants.CHALLENGE_MINUTES_OPTIONS[
                            constants.DEFAULT_CHALLENGE_MINUTE_OPTION_IDX
                        ] * 60,
                    incrementSeconds:
                        constants.CHALLENGE_INCREMENT_SECONDS_OPTIONS[
                            constants.DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX
                        ],
                },
            },
        });
        expect(routerMock.push).toHaveBeenCalledWith(
            `${constants.PATHS.CHALLENGE}/${challengeMock.challengeId}`,
        );
    });
});
