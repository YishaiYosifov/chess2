import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";
import {
    createFakeGuestUser,
    createFakePrivateUser,
} from "@/lib/testUtils/fakers/userFaker";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import { ChallengeRequest, GuestUser, PrivateUser } from "@/lib/apiClient";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import ChallengeDescription from "../ChallengeDescription";

describe("ChallengeDescription", () => {
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;
    let loggedInUserMock: PrivateUser;
    let guestUserMock: GuestUser;

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });

        loggedInUserMock = createFakePrivateUser({
            userId: challengeMock.requester.userId,
        });
        guestUserMock = createFakeGuestUser();
    });

    it("should render OpenChallengeView when logged-in user is requester and recipient is null", () => {
        challengeStore.setState({
            challenge: { ...challengeMock, recipient: null },
        });

        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("openChallengeViewInput"),
        ).toBeInTheDocument();
    });

    it("should render DirectChallengeView when logged-in user is requester and recipient exists", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("directChallengeViewUserName"),
        ).toBeInTheDocument();
    });

    it("should render RecipientChallengeView when logged-in user is not the requester", () => {
        const otherUserMock = createFakePrivateUser({
            userId: "differentUser123",
        });

        render(
            <SessionProvider user={otherUserMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("recipientChallengeViewUserName"),
        ).toBeInTheDocument();
    });

    it("should render RecipientChallengeView for guest users", () => {
        render(
            <SessionProvider user={guestUserMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("recipientChallengeViewUserName"),
        ).toBeInTheDocument();
    });
});
