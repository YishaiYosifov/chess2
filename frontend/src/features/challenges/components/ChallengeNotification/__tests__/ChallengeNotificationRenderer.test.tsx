import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ChallengeNotificationRenderer, {
    MAX_CHALLENGES,
} from "../ChallengeNotificationRenderer";
import {
    ChallengeClientEvents,
    useChallengeEvent,
} from "@/features/challenges/hooks/useChallengeHub";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { cancelAllIncomingChallenges } from "@/lib/apiClient";

vi.mock("@/features/challenges/hooks/useChallengeHub");
vi.mock("@/lib/apiClient/definition");

describe("ChallengeNotificationRenderer", () => {
    const cancelAllIncomingChallengesMock = vi.mocked(
        cancelAllIncomingChallenges,
    );

    const useChallengeEventMock = vi.mocked(useChallengeEvent);
    let challengeEventHandlers: EventHandlers<ChallengeClientEvents>;

    beforeEach(() => {
        vi.clearAllMocks();

        cancelAllIncomingChallengesMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });

        challengeEventHandlers = {};
        useChallengeEventMock.mockImplementation((event, handler) => {
            challengeEventHandlers[event] = handler;
        });
    });

    it("should render nothing if no challenges are received", () => {
        render(<ChallengeNotificationRenderer />);
        expect(
            screen.queryByTestId("challengeNotificationRendererCount"),
        ).not.toBeInTheDocument();
    });

    it("should add incoming challenges and display notification count", async () => {
        const challenge1 = createFakeChallengeRequest();
        const challenge2 = createFakeChallengeRequest();
        render(<ChallengeNotificationRenderer />);

        await act(() =>
            challengeEventHandlers["ChallengeReceivedAsync"]?.(challenge1),
        );
        await act(() =>
            challengeEventHandlers["ChallengeReceivedAsync"]?.(challenge2),
        );

        expect(
            screen.getByTestId(
                `challengeNotification-${challenge1.challengeId}`,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByTestId(
                `challengeNotification-${challenge2.challengeId}`,
            ),
        ).toBeInTheDocument();

        expect(
            screen.getByTestId("challengeNotificationRendererCount"),
        ).toHaveTextContent("2");
    });

    it("should move challenges to overflow when exceeding MAX_CHALLENGES", async () => {
        render(<ChallengeNotificationRenderer />);
        const challenges = Array.from({ length: 6 }, () =>
            createFakeChallengeRequest(),
        );

        await act(() =>
            challenges.map((ch) =>
                challengeEventHandlers["ChallengeReceivedAsync"]?.(ch),
            ),
        );

        for (let i = 0; i < MAX_CHALLENGES; i++) {
            expect(
                screen.getByTestId(
                    `challengeNotification-${challenges[i].challengeId}`,
                ),
            ).toBeInTheDocument();
        }

        expect(
            screen.queryByTestId(
                `challengeNotification-${challenges[5].challengeId}`,
            ),
        ).not.toBeInTheDocument();

        expect(
            screen.getByTestId("challengeNotificationRendererCount"),
        ).toHaveTextContent("6");
    });

    it("should promote overflow challenge when one incoming is removed", async () => {
        render(<ChallengeNotificationRenderer />);
        const challenges = Array.from({ length: 6 }, () =>
            createFakeChallengeRequest(),
        );

        await act(() =>
            challenges.map((challenge) =>
                challengeEventHandlers["ChallengeReceivedAsync"]?.(challenge),
            ),
        );

        await act(() =>
            challengeEventHandlers["ChallengeCancelledAsync"]?.(
                null,
                challenges[0].challengeId,
            ),
        );

        expect(
            screen.getByTestId(
                `challengeNotification-${challenges[5].challengeId}`,
            ),
        ).toBeInTheDocument();

        expect(
            screen.getByTestId("challengeNotificationRendererCount"),
        ).toHaveTextContent("5");
    });

    it("should cap notification count at 9+", async () => {
        render(<ChallengeNotificationRenderer />);
        const challenges = Array.from({ length: 10 }, () =>
            createFakeChallengeRequest(),
        );

        await act(() =>
            challenges.map((ch) =>
                challengeEventHandlers["ChallengeReceivedAsync"]?.(ch),
            ),
        );

        expect(
            screen.getByTestId("challengeNotificationRendererCount"),
        ).toHaveTextContent("9+");
    });

    it("should toggle show state on mouse enter and leave", async () => {
        const user = userEvent.setup();
        render(<ChallengeNotificationRenderer />);
        const challenge = createFakeChallengeRequest();

        await act(() =>
            challengeEventHandlers["ChallengeReceivedAsync"]?.(challenge),
        );

        const container = screen.getByTestId("challengeNotificationRenderer");

        expect(
            screen.getByTestId("challengeNotificationRendererList"),
        ).toBeInTheDocument();

        await user.hover(container);
        await user.unhover(container);
        expect(
            screen.queryByTestId("challengeNotificationRendererList"),
        ).not.toBeInTheDocument();

        await user.hover(container);
        expect(
            screen.getByTestId("challengeNotificationRendererList"),
        ).toBeInTheDocument();
    });

    it("should not show notifications when adding and overflowing", async () => {
        const user = userEvent.setup();
        render(<ChallengeNotificationRenderer />);

        for (let i = 0; i < MAX_CHALLENGES; i++) {
            await act(() =>
                challengeEventHandlers["ChallengeReceivedAsync"]?.(
                    createFakeChallengeRequest(),
                ),
            );
        }

        expect(
            screen.getByTestId("challengeNotificationRendererList"),
        ).toBeInTheDocument();

        const container = screen.getByTestId("challengeNotificationRenderer");

        // hover and unhover to hide
        await user.hover(container);
        await user.unhover(container);

        expect(
            screen.queryByTestId("challengeNotificationRendererList"),
        ).not.toBeInTheDocument();

        await act(() =>
            challengeEventHandlers["ChallengeReceivedAsync"]?.(
                createFakeChallengeRequest(),
            ),
        );

        expect(
            screen.queryByTestId("challengeNotificationRendererList"),
        ).not.toBeInTheDocument();
    });

    it("should cancel all incoming challenges when clicking decline all", async () => {
        const user = userEvent.setup();
        render(<ChallengeNotificationRenderer />);

        for (let i = 0; i < 5; i++) {
            await act(() =>
                challengeEventHandlers["ChallengeReceivedAsync"]?.(
                    createFakeChallengeRequest(),
                ),
            );
        }

        await user.click(
            screen.getByTestId("challengeNotificationRendererDeclineAll"),
        );

        expect(cancelAllIncomingChallengesMock).toHaveBeenCalledOnce();

        expect(
            screen.queryByTestId("challengeNotificationRenderer"),
        ).not.toBeInTheDocument();
    });
});
