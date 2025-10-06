import { act, render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequets } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { createFakeMinimalProfile } from "@/lib/testUtils/fakers/minimalProfileFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import DirectChallengeDescription from "../DirectChallengeDescription";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { ChallengeRequest, MinimalProfile, PrivateUser } from "@/lib/apiClient";

describe("DirectChallengeDescription", () => {
    let userMock: PrivateUser;
    let challengeMock: ChallengeRequest;
    let requesterMock: MinimalProfile;
    let recipientMock: MinimalProfile;

    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        userMock = createFakePrivateUser();
        requesterMock = createFakeMinimalProfile();
        recipientMock = createFakeMinimalProfile();
        challengeMock = createFakeChallengeRequets({
            requester: requesterMock,
            recipient: recipientMock,
        });

        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render RequesterPOV when user is the requester", () => {
        userMock.userId = requesterMock.userId;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <DirectChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("directChallengeDescriptionTitle"),
        ).toHaveTextContent("Waiting For");
        expect(
            screen.getByTestId("directChallengeDescriptionUserName"),
        ).toHaveTextContent(recipientMock.userName);
        expect(screen.getByTestId("profilePicture")).toBeInTheDocument();
    });

    it("should render RecipientPOV when user is the recipient", () => {
        userMock.userId = recipientMock.userId;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <DirectChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("directChallengeDescriptionTitle"),
        ).toHaveTextContent("Challenged By");
        expect(
            screen.getByTestId("challengeRecipientDescriptionUserName"),
        ).toHaveTextContent(requesterMock.userName);
    });

    it("should display 'Challenge Cancelled' when isCancelled is true", () => {
        userMock.userId = requesterMock.userId;
        challengeStore.setState({ isCancelled: true });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <DirectChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        const title = screen.getByTestId("directChallengeDescriptionTitle");
        expect(title).toHaveTextContent("Challenge Cancelled");
        expect(title).toHaveClass("text-error");
    });

    it("should display 'Challenge Expired' when hasExpired is true", () => {
        userMock.userId = requesterMock.userId;
        challengeStore.setState({ hasExpired: true });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <DirectChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        const title = screen.getByTestId("directChallengeDescriptionTitle");
        expect(title).toHaveTextContent("Challenge Expired");
        expect(title).toHaveClass("text-error");
    });

    it("should have 'animate-subtle-ping' only when not cancelled or expired", () => {
        userMock.userId = requesterMock.userId;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <DirectChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        const picture = screen.getByTestId("profilePicture");
        expect(picture).toHaveClass("animate-subtle-ping");

        act(() => challengeStore.setState({ hasExpired: true }));

        expect(screen.getByTestId("profilePicture")).not.toHaveClass(
            "animate-subtle-ping",
        );
    });
});
