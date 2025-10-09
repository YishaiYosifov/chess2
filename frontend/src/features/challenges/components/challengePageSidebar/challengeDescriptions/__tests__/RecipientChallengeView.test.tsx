import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import RecipientChallengeView from "../RecipientChallengeView";
import { ChallengeRequest } from "@/lib/apiClient";

describe("RecipientChallengeView", () => {
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render requester info", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <RecipientChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        expect(screen.getByTestId("profilePicture")).toBeInTheDocument();
        expect(
            screen.getByTestId("recipientChallengeViewUserName"),
        ).toHaveTextContent(challengeMock.requester.userName);
    });

    it("should render ChallengeStatusText with correct props", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <RecipientChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const statusText = screen.getByTestId("challengeStatusText");
        expect(statusText).toHaveTextContent("Challenged By");
        expect(statusText).toHaveClass("text-2xl");
    });

    it("should update correctly when challenge is over", () => {
        challengeStore.setState({ isExpired: true });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <RecipientChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const statusText = screen.getByTestId("challengeStatusText");
        expect(statusText).toHaveClass("text-error");
    });
});
