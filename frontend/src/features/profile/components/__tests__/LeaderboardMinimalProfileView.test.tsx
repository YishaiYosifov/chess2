import { render, screen } from "@testing-library/react";
import LeaderboardMinimalProfileView from "../LeaderboardMinimalProfileView";
import { MinimalProfile } from "@/lib/apiClient";
import { createFakeMinimalProfile } from "@/lib/testUtils/fakers/minimalProfileFaker";

describe("LeaderboardMinimalProfileView", () => {
    let profileMock: MinimalProfile;

    beforeEach(() => {
        profileMock = createFakeMinimalProfile();
    });

    it("should render the minimal profile inside the view", () => {
        render(
            <LeaderboardMinimalProfileView
                index={0}
                profile={profileMock}
                page={1}
                pageSize={1}
            />,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(profileMock.userName);
    });

    it.each([
        {
            page: 0,
            pageSize: 10,
            index: 0,
            expectedIcon: "🥇",
            expectedColor: "bg-amber-400",
        },
        {
            page: 0,
            pageSize: 10,
            index: 1,
            expectedIcon: "🥈",
            expectedColor: "bg-slate-300",
        },
        {
            page: 0,
            pageSize: 10,
            index: 2,
            expectedIcon: "🥉",
            expectedColor: "bg-orange-400",
        },
        {
            page: 0,
            pageSize: 10,
            index: 3,
            expectedIcon: "#4",
            expectedColor: "bg-text/70",
        },
        {
            page: 1,
            pageSize: 10,
            index: 0,
            expectedIcon: "#11",
            expectedColor: "bg-text/70",
        },
    ])(
        "should apply podium colors and icons correctly",
        ({ page, pageSize, index, expectedIcon, expectedColor }) => {
            render(
                <LeaderboardMinimalProfileView
                    profile={profileMock}
                    page={page}
                    pageSize={pageSize}
                    index={index}
                />,
            );

            const item = screen.getByTestId(
                `leaderboardItem-${profileMock.userId}`,
            );
            expect(item).toHaveTextContent(expectedIcon);
            expect(item).toHaveClass(expectedColor);
        },
    );
});
