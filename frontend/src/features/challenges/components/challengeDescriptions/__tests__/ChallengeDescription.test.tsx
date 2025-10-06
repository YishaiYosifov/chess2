import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import ChallengeDescription from "../ChallengeDescription";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { ChallengeRequest, PrivateUser } from "@/lib/apiClient";

describe("ChallengeDescription", () => {
    let userMock: PrivateUser;
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        userMock = createFakePrivateUser();
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render OpenChallengeDescription when there is no recipient", () => {
        challengeMock.recipient = null;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(screen.getByTestId("challengeStatusText")).toHaveTextContent(
            "Invite someone to play via:",
        );
    });

    it("should render DirectChallengeDescription when there is a recipient", () => {
        userMock.userId = challengeMock.requester.userId;

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(screen.getByTestId("challengeStatusText")).toHaveTextContent(
            "Waiting For", // waiting for recipient
        );
    });
});
