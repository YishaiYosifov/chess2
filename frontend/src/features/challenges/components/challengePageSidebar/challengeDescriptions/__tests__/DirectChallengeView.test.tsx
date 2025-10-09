import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { ChallengeRequest } from "@/lib/apiClient";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import DirectChallengeView from "../DirectChallengeView";

describe("DirectChallengeView", () => {
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render recipient info when recipient exists", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <DirectChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        expect(screen.getByTestId("profilePicture")).toBeInTheDocument();
        expect(
            screen.getByTestId("directChallengeViewUserName"),
        ).toHaveTextContent(challengeMock.recipient!.userName);
    });

    it("should apply animation class when challenge is active", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <DirectChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const profilePicture = screen.getByTestId("profilePicture");
        expect(profilePicture.className).toContain("animate-subtle-ping");
    });

    it("should update correctly when challenge is over", () => {
        challengeStore.setState({ isCancelled: true });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <DirectChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const profilePicture = screen.getByTestId("profilePicture");
        expect(profilePicture.className).not.toContain("animate-subtle-ping");

        const statusText = screen.getByTestId("challengeStatusText");
        expect(statusText).toHaveClass("text-error");
    });

    it("should render ChallengeStatusText with correct props", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <DirectChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const statusText = screen.getByTestId("challengeStatusText");
        expect(statusText).toHaveTextContent("Waiting For");
        expect(statusText).toHaveClass("text-2xl");
    });
});
