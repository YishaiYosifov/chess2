import SessionProvider from "@/features/auth/contexts/sessionContext";
import { ChallengeRequest, PrivateUser } from "@/lib/apiClient";
import { StoreApi } from "zustand";
import ChallengeStoreContext from "../../contexts/challengeContext";
import ChallengeStatusText from "../ChallengeStatusText";
import { render, screen } from "@testing-library/react";
import {
    ChallengeStore,
    createChallengeStore,
} from "../../stores/challengeStore";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";

describe("ChallengeStatusText", () => {
    let userMock: PrivateUser;
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        userMock = createFakePrivateUser();
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    const renderWithProviders = () =>
        render(
            <SessionProvider user={userMock}>
                <ChallengeStoreContext.Provider value={challengeStore}>
                    <ChallengeStatusText
                        activeText="Active Challenge"
                        activeClassName="text-active"
                        overClassName="text-over"
                    />
                </ChallengeStoreContext.Provider>
            </SessionProvider>,
        );

    it("should render active text when not cancelled or expired", () => {
        renderWithProviders();

        const text = screen.getByText("Active Challenge");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-active");
    });

    it("should render 'Challenge Expired' when isExpired is true", () => {
        challengeStore.setState({ isExpired: true });
        renderWithProviders();

        const text = screen.getByText("Challenge Expired");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });

    it("should render 'Challenge Cancelled' when isCancelled is true and cancelled by current user", () => {
        challengeStore.setState({
            isCancelled: true,
            cancelledBy: userMock.userId,
        });
        renderWithProviders();

        const text = screen.getByText("Challenge Cancelled");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });

    it("should render 'Challenge Declined' when isCancelled is true and cancelled by opponent", () => {
        challengeStore.setState({
            isCancelled: true,
            cancelledBy: "someOtherUserId",
        });
        renderWithProviders();

        const text = screen.getByText("Challenge Declined");
        expect(text).toBeInTheDocument();
        expect(text).toHaveClass("text-over");
    });
});
