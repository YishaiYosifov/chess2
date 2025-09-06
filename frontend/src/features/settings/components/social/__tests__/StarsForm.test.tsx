import { render, screen, waitFor } from "@testing-library/react";

import {
    addStar,
    getStarredUsers,
    PagedResultOfMinimalProfile,
    removeStar,
} from "@/lib/apiClient";
import StarsForm from "../StarsForm";
import userEvent from "@testing-library/user-event";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import {
    createFakeMinimalProfile,
    createFakePagedStars,
} from "@/lib/testUtils/fakers/minimalProfileFaker";

vi.mock("@/lib/apiClient/definition");

describe("StarsForm", () => {
    const userMock = createFakePrivateUser();

    const getStarredUsersMock = vi.mocked(getStarredUsers);
    const addStarMock = vi.mocked(addStar);
    const removeStarMock = vi.mocked(removeStar);

    beforeEach(() => {
        vi.resetAllMocks();
        getStarredUsersMock.mockResolvedValue({
            data: createFakePagedStars(),
            response: new Response(),
        });
        addStarMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        removeStarMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should render the stars list with initial data", () => {
        const initial = createFakePagedStars({
            pagination: { pageSize: 2, totalCount: 2, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("starsFormHeading")).toBeInTheDocument();
        expect(screen.getByTestId("starsFormCount")).toHaveTextContent(
            `You starred ${initial.totalCount} players`,
        );
        expect(screen.getAllByTestId(/starsFormToggleStarButton/)).toHaveLength(
            2,
        );
    });

    it("should toggle star on a profile when button clicked", async () => {
        const user = userEvent.setup();
        const profile = createFakeMinimalProfile();
        const initial = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 1, page: 0 },
            overrides: profile,
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        const button = screen.getByTestId(
            `starsFormToggleStarButton${profile.userId}`,
        );

        await user.click(button);
        expect(removeStarMock).toHaveBeenCalledWith({
            path: { starredUserId: profile.userId },
        });
        expect(
            screen.getByTestId("starsFormStarButtonLabel"),
        ).toHaveTextContent("Star");

        await user.click(button);
        expect(addStarMock).toHaveBeenCalledWith({
            path: { starredUserId: profile.userId },
        });
    });

    it("should fetch new page and update stars when pagination button clicked", async () => {
        const user = userEvent.setup();
        const initial = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 5, page: 0 },
        });

        const newStars = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 5, page: 1 },
        });

        getStarredUsersMock.mockResolvedValueOnce({
            data: newStars,
            response: new Response(),
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        const nextBtn = screen.getByTestId("paginationNext");
        await user.click(nextBtn);

        expect(getStarredUsersMock).toHaveBeenCalledWith({
            path: { userId: userMock.userId },
            query: { Page: 1, PageSize: initial.pageSize },
        });

        const renderedProfiles = screen.getAllByTestId(/starsFormStarProfile/);
        expect(renderedProfiles).toHaveLength(newStars.items.length);
        newStars.items.forEach((profile) => {
            expect(screen.getByText(profile.userName)).toBeInTheDocument();
        });
    });

    it("should disable pagination buttons while fetching", async () => {
        const user = userEvent.setup();
        const initial = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 5, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        type GetGameResultsReturnType = {
            data: PagedResultOfMinimalProfile;
            response: Response;
        };
        let resolveFetch!: (value: GetGameResultsReturnType) => void;
        const fetchPromise = new Promise<GetGameResultsReturnType>(
            (resolve) => {
                resolveFetch = resolve;
            },
        );
        getStarredUsersMock.mockReturnValue(fetchPromise);

        const nextBtn = screen.getByTestId("paginationNext");
        await user.click(nextBtn);

        expect(nextBtn).toBeDisabled();
        resolveFetch({ data: initial, response: new Response() });
        await waitFor(() => expect(getStarredUsersMock).toHaveBeenCalled());
        expect(nextBtn).toBeEnabled();
    });

    it("should have profile link pointing to correct URL", () => {
        const initial = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 1, page: 0 },
        });
        const profile = initial.items[0];

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        const link = screen.getByTestId(
            `starsFormProfileLink${profile.userId}`,
        );
        expect(link).toHaveAttribute("href", `/profile/${profile.userName}`);
    });
});
