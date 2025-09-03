import { render, screen } from "@testing-library/react";

import {
    getGameResults,
    getHasStarred,
    getRatingArchives,
    getStarredUsers,
    getStarsReceivedCount,
    getUser,
    MinimalProfile,
    PagedResultOfGameSummaryDto,
    PrivateUser,
    PublicUser,
    RatingOverview,
} from "@/lib/apiClient";
import {
    createFakePrivateUser,
    createFakeUser,
} from "@/lib/testUtils/fakers/userFaker";
import { LoadProfilePage } from "../page";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { createFakeEmptyPagedResult } from "@/lib/testUtils/fakers/pagedResultFaker";
import constants from "@/lib/constants";
import { createFakePagedGameSummary } from "@/lib/testUtils/fakers/pagedGameSummaryFaker";
import { createFakeRatingOverview } from "@/lib/testUtils/fakers/ratingOverviewFaker";

vi.mock("@/lib/apiClient/definition");

describe("ProfilePage", () => {
    let ownUser: PrivateUser;
    let otherUser: PublicUser;
    let gameSummary: PagedResultOfGameSummaryDto;
    let rating: RatingOverview;

    const starsReceived = 5;

    const getUserMock = vi.mocked(getUser);
    const getRatingArchivesMock = vi.mocked(getRatingArchives);
    const getGameResultsMock = vi.mocked(getGameResults);
    const getStarredUsersMock = vi.mocked(getStarredUsers);
    const getStarsReceivedCountMock = vi.mocked(getStarsReceivedCount);
    const getHasStarredMock = vi.mocked(getHasStarred);

    beforeEach(() => {
        vi.resetAllMocks();

        ownUser = createFakePrivateUser();
        otherUser = createFakeUser();
        gameSummary = createFakePagedGameSummary({
            pagination: { totalCount: 1 },
        });
        rating = createFakeRatingOverview({
            timeControl: constants.DISPLAY_TIME_CONTROLS[0],
        });

        getUserMock.mockResolvedValue({
            data: otherUser,
            response: new Response(),
        });
        getRatingArchivesMock.mockResolvedValue({
            data: [rating],
            response: new Response(),
        });
        getGameResultsMock.mockResolvedValue({
            data: gameSummary,
            response: new Response(),
        });
        getStarredUsersMock.mockResolvedValue({
            data: createFakeEmptyPagedResult<MinimalProfile>(
                constants.PAGINATION_PAGE_SIZE.STARS,
            ),
            response: new Response(),
        });
        getStarsReceivedCountMock.mockResolvedValue({
            data: starsReceived,
            response: new Response(),
        });
        getHasStarredMock.mockResolvedValue({
            data: false,
            response: new Response(),
        });
    });

    it("should render own profile without calling getUser", async () => {
        render(
            <SessionProvider user={ownUser}>
                {await LoadProfilePage({
                    loggedInUser: ownUser,
                    accessToken: "token",
                    profileUsername: ownUser.userName,
                })}
            </SessionProvider>,
        );

        expect(screen.getByTestId("profileUsername")).toHaveTextContent(
            ownUser.userName,
        );
        expect(getUserMock).not.toHaveBeenCalled();
    });

    it("should fetch profile when viewing someone else's page", async () => {
        render(
            <SessionProvider user={ownUser}>
                {await LoadProfilePage({
                    loggedInUser: ownUser,
                    accessToken: "token",
                    profileUsername: otherUser.userName,
                })}
            </SessionProvider>,
        );

        expect(getUserMock).toHaveBeenCalledWith({
            path: { username: otherUser.userName },
        });
        expect(screen.getByTestId("profileUsername")).toHaveTextContent(
            otherUser.userName,
        );
    });

    it("should pass API data to Profile and GameHistory components", async () => {
        render(
            <SessionProvider user={ownUser}>
                {await LoadProfilePage({
                    loggedInUser: ownUser,
                    accessToken: "token",
                    profileUsername: otherUser.userName,
                })}
            </SessionProvider>,
        );

        expect(screen.getByTestId("profileUsername")).toHaveTextContent(
            otherUser.userName,
        );
        expect(screen.getByTestId("profileStarCount")).toHaveTextContent(
            starsReceived.toString(),
        );

        expect(
            screen.getByTestId(
                `ratingCard-${constants.DISPLAY_TIME_CONTROLS[0]}`,
            ),
        ).toBeInTheDocument();
        expect(
            screen.queryByTestId(
                `emptyRatingCard-${constants.DISPLAY_TIME_CONTROLS[0]}`,
            ),
        ).not.toBeInTheDocument();
        for (const timeControl of constants.DISPLAY_TIME_CONTROLS.slice(1)) {
            expect(
                screen.getByTestId(`emptyRatingCard-${timeControl}`),
            ).toBeInTheDocument();
            expect(
                screen.queryByTestId(`ratingCard-${timeControl}`),
            ).not.toBeInTheDocument();
        }
    });

    it.each([
        [true, "Unstar"],
        [false, "Star"],
    ])(
        "should set initialHasStarred=%s and render button text '%s'",
        async (hasStarred, expectedText) => {
            // Mock getHasStarred for this run
            getHasStarredMock.mockResolvedValueOnce({
                data: hasStarred,
                response: new Response(),
            });

            render(
                <SessionProvider user={ownUser}>
                    {await LoadProfilePage({
                        loggedInUser: ownUser,
                        accessToken: "token",
                        profileUsername: otherUser.userName,
                    })}
                </SessionProvider>,
            );

            const starButton = screen.getByTestId("profileStarButton");
            expect(starButton).toHaveTextContent(expectedText);
        },
    );

    it("should not call getHasStarred if the user is not logged in", async () => {
        render(
            <SessionProvider user={null} fetchAttempted>
                {await LoadProfilePage({
                    loggedInUser: null,
                    accessToken: null,
                    profileUsername: otherUser.userName,
                })}
            </SessionProvider>,
        );

        expect(getHasStarredMock).not.toHaveBeenCalled();
        const starButton = screen.getByTestId("profileStarButton");
        expect(starButton).toHaveTextContent("Star");
    });
});
