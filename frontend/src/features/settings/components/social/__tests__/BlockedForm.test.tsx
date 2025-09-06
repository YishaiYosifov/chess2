import { render, screen } from "@testing-library/react";

import SessionProvider from "@/features/auth/contexts/sessionContext";
import BlockedForm from "../BlockedForm";
import userEvent from "@testing-library/user-event";
import {
    createFakeMinimalProfile,
    createFakePagedBlocked,
} from "@/lib/testUtils/fakers/minimalProfileFaker";
import {
    blockUser,
    getBlockedUsers,
    PrivateUser,
    unblockUser,
} from "@/lib/apiClient";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";

vi.mock("@/lib/apiClient/definition");

describe("BlockedForm", () => {
    let userMock: PrivateUser;

    const getBlockedUsersMock = vi.mocked(getBlockedUsers);
    const blockUserMock = vi.mocked(blockUser);
    const unblockUserMock = vi.mocked(unblockUser);

    beforeEach(() => {
        userMock = createFakePrivateUser();
        getBlockedUsersMock.mockResolvedValue({
            data: createFakePagedBlocked(),
            response: new Response(),
        });

        blockUserMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        unblockUserMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should render heading and blocked count from initial data", () => {
        const initial = createFakePagedBlocked({
            pagination: { pageSize: 2, totalCount: 2, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <BlockedForm initialBlocked={initial} />
            </SessionProvider>,
        );

        expect(screen.getByText("Blocked")).toBeInTheDocument();
        expect(screen.getByTestId("blockedFormCount")).toHaveTextContent(
            `You blocked ${initial.totalCount} players`,
        );
    });

    it("should call unblock and block when toggling a profile", async () => {
        const user = userEvent.setup();
        const profile = createFakeMinimalProfile();
        const initial = createFakePagedBlocked({
            pagination: { pageSize: 1, totalCount: 1, page: 0 },
            overrides: profile,
        });

        render(
            <SessionProvider user={userMock}>
                <BlockedForm initialBlocked={initial} />
            </SessionProvider>,
        );

        const button = screen.getByTestId("relationProfileRowToggle");

        await user.click(button);
        expect(unblockUserMock).toHaveBeenCalledWith({
            path: { blockedUserId: profile.userId },
        });

        await user.click(button);
        expect(blockUserMock).toHaveBeenCalledWith({
            path: { blockedUserId: profile.userId },
        });
    });

    it("should toggle profile action button correctly", async () => {
        const user = userEvent.setup();
        const initial = createFakePagedBlocked({
            pagination: { pageSize: 1, totalCount: 1, page: 0 },
        });

        render(
            <SessionProvider user={userMock}>
                <BlockedForm initialBlocked={initial} />
            </SessionProvider>,
        );

        const button = screen.getByTestId("relationProfileRowToggle");

        await user.click(button);
        expect(
            screen.getByTestId("relationProfileRowToggle"),
        ).toHaveTextContent("Block");

        await user.click(button);
        expect(
            screen.getByTestId("relationProfileRowToggle"),
        ).toHaveTextContent("Unblock");
    });

    it("should fetch new blocked users when pagination is triggered", async () => {
        const user = userEvent.setup();
        const initial = createFakePagedBlocked({
            pagination: { pageSize: 1, totalCount: 5, page: 0 },
        });

        const newBlocked = createFakePagedBlocked({
            pagination: { pageSize: 1, totalCount: 5, page: 1 },
        });

        getBlockedUsersMock.mockResolvedValueOnce({
            data: newBlocked,
            response: new Response(),
        });

        render(
            <SessionProvider user={userMock}>
                <BlockedForm initialBlocked={initial} />
            </SessionProvider>,
        );

        const nextBtn = screen.getByTestId("paginationNext");
        await user.click(nextBtn);

        expect(getBlockedUsersMock).toHaveBeenCalledWith({
            query: { Page: 1, PageSize: initial.pageSize },
        });
        expect(screen.getByTestId("blockedFormCount")).toHaveTextContent(
            `You blocked ${newBlocked.totalCount} players`,
        );
    });
});
