import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequets } from "@/lib/testUtils/fakers/challengeRequestFaker";
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
        challengeMock = createFakeChallengeRequets();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render OpenChallengeDescription when there is no recipient", () => {
        challengeMock.recipient = null;
        challengeStore = createChallengeStore({ challenge: challengeMock });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("openChallengeDescriptionTitle"),
        ).toBeInTheDocument();
        expect(
            screen.queryByTestId("directChallengeDescriptionTitle"),
        ).not.toBeInTheDocument();
    });

    it("should render DirectChallengeDescription when there is a recipient", () => {
        challengeStore = createChallengeStore({ challenge: challengeMock });

        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeDescription />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("directChallengeDescriptionTitle"),
        ).toBeInTheDocument();
        expect(
            screen.queryByTestId("openChallengeDescriptionTitle"),
        ).not.toBeInTheDocument();
    });
});
