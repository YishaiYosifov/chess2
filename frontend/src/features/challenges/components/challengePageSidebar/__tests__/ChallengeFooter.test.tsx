import { act, render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import ChallengeFooter from "../ChallengeFooter";
import { ChallengeRequest, PoolType, PrivateUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";

describe("ChallengeFooter", () => {
    let challengeStore: StoreApi<ChallengeStore>;
    let challengeMock: ChallengeRequest;
    let userMock: PrivateUser;

    beforeEach(() => {
        userMock = createFakePrivateUser();
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({
            challenge: challengeMock,
        });
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    it("should render Cancel button when user is requester", () => {
        userMock.userId = challengeMock.requester.userId;
        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("challengeFooterCancelButton"),
        ).toBeInTheDocument();
    });

    it("should render RecipientActions when user is recipient", () => {
        userMock.userId = challengeMock.recipient!.userId;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("challengeFooterAcceptButton"),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId("challengeFooterDeclineButton"),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId("challengeFooterRecipientPrompt"),
        ).toBeInTheDocument();
    });

    it("should render ChallengeOver when challenge is cancelled", () => {
        challengeStore.setState({ isCancelled: true });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("challengeFooterNewOpponent"),
        ).toBeInTheDocument();
    });

    it("should render ChallengeOver when challenge is expired", () => {
        challengeStore.setState({ isExpired: true });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("challengeFooterNewOpponent"),
        ).toHaveAttribute("href", constants.PATHS.PLAY);
    });

    it("should update countdown text over time", () => {
        const now = Date.now();
        vi.setSystemTime(now);
        challengeMock.expiresAt = new Date(now + 60000).toISOString();
        challengeStore.setState({ challenge: challengeMock });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        const countdown = screen.getByTestId("challengeFooterExpiresIn");
        expect(countdown).toBeInTheDocument();
        expect(countdown.textContent).toBe("Expires in 00:01:00");
    });

    it("should mark as expired once expired", async () => {
        const now = Date.now();
        vi.setSystemTime(now);
        challengeMock.expiresAt = new Date(now + 60000).toISOString();
        challengeStore.setState({ challenge: challengeMock });

        expect(challengeStore.getState().isExpired).toBe(false);

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        await act(() => vi.advanceTimersByTime(61000));

        expect(challengeStore.getState().isExpired).toBe(true);
    });

    it("should disable Accept button and show guest message for rated challenge when user is guest", () => {
        challengeMock.pool.poolType = PoolType.RATED;
        challengeStore.setState({ challenge: challengeMock });

        render(
            <SessionProvider user={null} fetchAttempted>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeFooter />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        const acceptButton = screen.getByTestId("challengeFooterAcceptButton");

        expect(acceptButton).toBeInTheDocument();
        expect(acceptButton).toBeDisabled();
        expect(acceptButton).toHaveTextContent(
            "Guests can't accept rated challenges",
        );

        expect(
            screen.getByTestId("challengeFooterRecipientPrompt"),
        ).toBeInTheDocument();
    });
});
