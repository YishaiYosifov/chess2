import { StoreApi } from "zustand";
import {
    ChallengeStore,
    createChallengeStore,
} from "../../stores/challengeStore";
import { ChallengeRequest } from "@/lib/apiClient";
import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import {
    ChallengeClientEvents,
    useChallengeInstanceEvent,
} from "../useChallengeHub";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import { act, renderHook } from "@testing-library/react";
import useChallengeEvents from "../useChallengeEvents";
import constants from "@/lib/constants";

vi.mock("@/features/challenges/hooks/useChallengeHub");

describe("useChallengeEvents", () => {
    let challengeStore: StoreApi<ChallengeStore>;
    let challengeMock: ChallengeRequest;

    const useChallengeInstanceEventMock = vi.mocked(useChallengeInstanceEvent);
    const challengeEventHandlers: EventHandlers<ChallengeClientEvents> = {};

    beforeEach(() => {
        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });

        useChallengeInstanceEventMock.mockImplementation(
            (challengeToken, event, handler) => {
                if (challengeToken === challengeMock.challengeToken)
                    challengeEventHandlers[event] = handler;
            },
        );
    });

    it("should redirect when ChallengeAcceptedAsync with the right challenge id", async () => {
        const routerMock = mockRouter();
        const gameToken = "test game token";
        renderHook(() =>
            useChallengeEvents(challengeStore, challengeMock.challengeToken),
        );

        await act(() =>
            challengeEventHandlers["ChallengeAcceptedAsync"]?.(
                gameToken,
                challengeMock.challengeToken,
            ),
        );

        expect(routerMock.push).toHaveBeenCalledExactlyOnceWith(
            `${constants.PATHS.GAME}/${gameToken}`,
        );
    });

    it("should not redirect when ChallengeAcceptedAsync with the wrong challenge id", async () => {
        const routerMock = mockRouter();
        renderHook(() =>
            useChallengeEvents(challengeStore, challengeMock.challengeToken),
        );

        await act(() =>
            challengeEventHandlers["ChallengeAcceptedAsync"]?.(
                "test game token",
                "some random challenge",
            ),
        );

        expect(routerMock.push).not.toHaveBeenCalled();
    });

    it("should mark as cancelled when ChallengeCancelledAsync with the right challenge id", async () => {
        const cancelledBy = "cancelled by";
        renderHook(() =>
            useChallengeEvents(challengeStore, challengeMock.challengeToken),
        );

        await act(() =>
            challengeEventHandlers["ChallengeCancelledAsync"]?.(
                cancelledBy,
                challengeMock.challengeToken,
            ),
        );

        expect(challengeStore.getState().isCancelled).toBe(true);
        expect(challengeStore.getState().cancelledBy).toBe(cancelledBy);
    });

    it("should not do anything when ChallengeCancelledAsync with the wrong challenge id", async () => {
        renderHook(() =>
            useChallengeEvents(challengeStore, challengeMock.challengeToken),
        );

        await act(() =>
            challengeEventHandlers["ChallengeCancelledAsync"]?.(
                "cancelled by",
                "some random challenge",
            ),
        );

        expect(challengeStore.getState().isCancelled).toBe(false);
        expect(challengeStore.getState().cancelledBy).toBe(null);
    });
});
