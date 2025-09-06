import { render, screen } from "@testing-library/react";

import {
    addStar,
    getStarredUsers,
    PrivateUser,
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
    let userMock: PrivateUser;

    const getStarredUsersMock = vi.mocked(getStarredUsers);
    const addStarMock = vi.mocked(addStar);
    const removeStarMock = vi.mocked(removeStar);

    beforeEach(() => {
        userMock = createFakePrivateUser();
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

    it("should render heading and count from initial stars", () => {
        const initial = createFakePagedStars({
            pagination: { pageSize: 2, totalCount: 2, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        expect(screen.getByTestId("starsFormHeading")).toHaveTextContent(
            "Stars",
        );
        expect(screen.getByTestId("starsFormCount")).toHaveTextContent(
            `You starred ${initial.totalCount} players`,
        );
    });

    it("should call removeStar and addStar when toggling", async () => {
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

        const button = screen.getByTestId("relationProfileRowToggle");

        await user.click(button);
        expect(removeStarMock).toHaveBeenCalledWith({
            path: { starredUserId: profile.userId },
        });

        await user.click(button);
        expect(addStarMock).toHaveBeenCalledWith({
            path: { starredUserId: profile.userId },
        });
    });

    it("should toggle profile action button correctly", async () => {
        const user = userEvent.setup();
        const initial = createFakePagedStars({
            pagination: { pageSize: 1, totalCount: 1, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <StarsForm initialStars={initial} />
            </SessionProvider>,
        );

        const button = screen.getByTestId("relationProfileRowToggle");

        await user.click(button);
        expect(
            screen.getByTestId("starsFormStarIconOutline"),
        ).toBeInTheDocument();
        expect(
            screen.queryByTestId("starsFormStarIconSolid"),
        ).not.toBeInTheDocument();
        expect(
            screen.getByTestId("relationProfileRowToggle"),
        ).toHaveTextContent("Star");

        await user.click(button);
        expect(
            screen.getByTestId("starsFormStarIconSolid"),
        ).toBeInTheDocument();
        expect(
            screen.queryByTestId("starsFormStarIconOutline"),
        ).not.toBeInTheDocument();
        expect(
            screen.getByTestId("relationProfileRowToggle"),
        ).toHaveTextContent("Starred");
    });

    it("should fetch new stars when pagination is triggered", async () => {
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

        expect(screen.getByTestId("starsFormCount")).toHaveTextContent(
            `You starred ${newStars.totalCount} players`,
        );
    });
});
