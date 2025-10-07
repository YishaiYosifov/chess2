import { render, screen, within } from "@testing-library/react";
import {
    getCurrentRatings,
    getUserByUsername,
    TimeControl,
} from "@/lib/apiClient";
import { createFakeCurrentRatingStatus } from "@/lib/testUtils/fakers/currentRatingStatusFaker";
import { createFakeUser } from "@/lib/testUtils/fakers/userFaker";
import userEvent from "@testing-library/user-event";
import ProfileTooltip from "../ProfileTooltip";

vi.mock("@/lib/apiClient/definition");

describe("ProfileTooltip", () => {
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
            <ProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </ProfileTooltip>,
        );

        await user.click(screen.getByTestId("profileTooltipChildren"));

        const tooltip = await screen.findByTestId("profileTooltip");
        expect(tooltip).toBeInTheDocument();
        expect(screen.getByTestId("profileTooltipUsername")).toHaveTextContent(
            userMock.userName,
        );
        expect(screen.getByTestId("profileTooltipAbout")).toHaveTextContent(
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
            <ProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </ProfileTooltip>,
        );

        await user.click(screen.getByTestId("profileTooltipChildren"));

        for (const rating of ratingsMock) {
            const container = await screen.findByTestId(
                `profileTooltipRating-${rating.timeControl}`,
            );
            const value = screen.getByTestId(
                `profileTooltipRatingValue-${rating.timeControl}`,
            );

            expect(container).toBeInTheDocument();
            expect(value).toHaveTextContent(String(rating.rating));
        }
    });

    it("should not refetch when reopening tooltip", async () => {
        const user = userEvent.setup();

        render(
            <ProfileTooltip username={userMock.userName}>
                <div>Trigger</div>
            </ProfileTooltip>,
        );

        const trigger = screen.getByTestId("profileTooltipChildren");

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
                <ProfileTooltip username={userMock.userName}>
                    <div>Trigger</div>
                </ProfileTooltip>
            </>,
        );

        await user.click(screen.getByTestId("profileTooltipChildren"));

        expect(await screen.findByTestId("profileTooltip")).toBeInTheDocument();

        await user.click(screen.getByTestId("outside"));

        expect(screen.queryByTestId("profileTooltip")).not.toBeInTheDocument();
    });
});
