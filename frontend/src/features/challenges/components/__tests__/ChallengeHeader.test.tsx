import { ChallengeRequest, PoolType } from "@/lib/apiClient";
import { StoreApi } from "zustand";
import {
    ChallengeStore,
    createChallengeStore,
} from "../../stores/challengeStore";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { render, screen } from "@testing-library/react";
import ChallengeStoreContext from "../../contexts/challengeContext";
import ChallengeHeader from "../ChallengeHeader";

describe("ChallengeHeader", () => {
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render the challenge title", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <ChallengeHeader />
            </ChallengeStoreContext.Provider>,
        );

        expect(screen.getByTestId("challengeHeaderTitle")).toHaveTextContent(
            "Challenge",
        );
    });

    it("should display the correct time control and increment", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <ChallengeHeader />
            </ChallengeStoreContext.Provider>,
        );

        const expectedText = `${challengeMock.pool.timeControl.baseSeconds / 60}+${challengeMock.pool.timeControl.incrementSeconds}`;
        expect(
            screen.getByTestId("challengeHeaderTimeControl"),
        ).toHaveTextContent(expectedText);
    });

    it("should show 'Rated' when pool type is rated", () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <ChallengeHeader />
            </ChallengeStoreContext.Provider>,
        );

        expect(
            screen.getByTestId("challengeHeaderTimeControl"),
        ).toHaveTextContent("Rated");
    });

    it("should show 'Casual' when pool type is casual", () => {
        challengeMock.pool.poolType = PoolType.CASUAL;

        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <ChallengeHeader />
            </ChallengeStoreContext.Provider>,
        );

        expect(
            screen.getByTestId("challengeHeaderTimeControl"),
        ).toHaveTextContent("Casual");
    });
});
