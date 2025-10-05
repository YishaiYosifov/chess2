import { render, screen, within } from "@testing-library/react";
import {
    getCurrentRatings,
    getUserByUsername,
    TimeControl,
} from "@/lib/apiClient";
import { createFakeCurrentRatingStatus } from "@/lib/testUtils/fakers/currentRatingStatusFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import userEvent from "@testing-library/user-event";
import UserProfileTooltip from "../UserProfileTooltip";

vi.mock("@/lib/apiClient/definition");

describe("UserProfileTooltip", () => {
    const userMock = createFakeUser();
    const ratingsMock = [
        createFakeCurrentRatingStatus({ timeControl: TimeControl.BLITZ }),
        createFakeCurrentRatingStatus({ timeControl: TimeControl.BULLET }),
    ];

    const getUserByUsernameMock = vi.mocked(getUserByUsername);
    const getCurrentRatingsMock = vi.mocked(getCurrentRatings);

    beforeEach(() => {
        getUserByUsernameMock.mockResolvedValue({
            data: userMock,
            response: new Response(),
        });
        getCurrentRatingsMock.mockResolvedValue({
            data: ratingsMock,
            response: new Response(),
        });
    });

    it("should load and render profile info, about text, profile picture and flag in tooltip", async () => {
        const user = userEvent.setup();

        render(
            <UserProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </UserProfileTooltip>,
        );

        await user.click(screen.getByTestId("userProfileTooltipChildren"));

        const tooltip = await screen.findByTestId("userProfileTooltip");
        expect(tooltip).toBeInTheDocument();
        expect(
            screen.getByTestId("userProfileTooltipUsername"),
        ).toHaveTextContent(userMock.userName);
        expect(screen.getByTestId("userProfileTooltipAbout")).toHaveTextContent(
            userMock.about,
        );

        expect(
            within(tooltip).getByTestId("profilePicture"),
        ).toBeInTheDocument();
        expect(within(tooltip).getByTestId("flag")).toBeInTheDocument();

        expect(getUserByUsernameMock).toHaveBeenCalledExactlyOnceWith({
            path: { username: userMock.userName },
        });
        expect(getCurrentRatingsMock).toHaveBeenCalledExactlyOnceWith({
            path: { userId: userMock.userId },
        });
    });

    it("should render all ratings in tooltip", async () => {
        const user = userEvent.setup();
        render(
            <UserProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </UserProfileTooltip>,
        );

        await user.click(screen.getByTestId("userProfileTooltipChildren"));

        for (const rating of ratingsMock) {
            const container = await screen.findByTestId(
                `userProfileTooltipRating-${rating.timeControl}`,
            );
            const value = screen.getByTestId(
                `userProfileTooltipRatingValue-${rating.timeControl}`,
            );

            expect(container).toBeInTheDocument();
            expect(value).toHaveTextContent(String(rating.rating));
        }
    });

    it("should not refetch when reopening tooltip", async () => {
        const user = userEvent.setup();

        render(
            <UserProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </UserProfileTooltip>,
        );

        const trigger = screen.getByTestId("userProfileTooltipChildren");

        await user.click(trigger);
        await user.click(trigger);
        await user.click(trigger);

        expect(getUserByUsername).toHaveBeenCalledOnce();
        expect(getCurrentRatings).toHaveBeenCalledOnce();
    });

    it("should close tooltip when clicking outside", async () => {
        const user = userEvent.setup();

        render(
            <>
                <div data-testid="outside">Outside</div>
                <UserProfileTooltip username={userMock.userName}>
                    <div>Trigger</div>
                </UserProfileTooltip>
            </>,
        );

        await user.click(screen.getByTestId("userProfileTooltipChildren"));

        expect(
            await screen.findByTestId("userProfileTooltip"),
        ).toBeInTheDocument();

        await user.click(screen.getByTestId("outside"));

        expect(
            screen.queryByTestId("userProfileTooltip"),
        ).not.toBeInTheDocument();
    });
});
