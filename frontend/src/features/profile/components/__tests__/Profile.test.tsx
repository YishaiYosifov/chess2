import { render, screen } from "@testing-library/react";

import {
    createFakePrivateUser,
    createFakeUser,
} from "@/lib/testUtils/fakers/userFaker";
import { addStar, PrivateUser, PublicUser, removeStar } from "@/lib/apiClient";
import Profile from "../Profile";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import userEvent from "@testing-library/user-event";
import constants from "@/lib/constants";

vi.mock("next/image");
vi.mock("@/lib/apiClient/definition");

describe("Profile", () => {
    let userMock: PublicUser;
    let loggedInUserMock: PrivateUser;

    const addStarMock = vi.mocked(addStar);
    const removeStarMock = vi.mocked(removeStar);

    beforeEach(() => {
        userMock = createFakeUser();
        loggedInUserMock = createFakePrivateUser();

        addStarMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
        removeStarMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    function renderProfile({
        isLoggedOut,
        starCount,
        questPoints,
    }: {
        isLoggedOut?: boolean;
        starCount?: number;
        questPoints?: number;
    } = {}) {
        return render(
            <SessionProvider
                user={isLoggedOut ? null : loggedInUserMock}
                fetchAttempted
            >
                <Profile
                    profile={userMock}
                    questPoints={questPoints ?? 0}
                    initialStarCount={starCount ?? 0}
                    initialHasStarred={false}
                    initialHasBlocked={false}
                />
            </SessionProvider>,
        );
    }

    it("should render the profile correctly", () => {
        renderProfile({ starCount: 5 });

        expect(screen.getByAltText("profile picture")).toBeInTheDocument();
        expect(screen.getByTestId("username")).toBeInTheDocument();
        expect(screen.getByTestId("aboutMe")).toBeInTheDocument();
        expect(screen.getByTestId("profileStarCount")).toHaveTextContent("5");
        expect(screen.getByText(/Joined/i)).toBeInTheDocument();
    });

    it("should display the username and flag correctly", () => {
        renderProfile();

        expect(screen.getByText(userMock.userName!)).toBeInTheDocument();
        const flag = screen.getByTestId("flag");
        expect(flag).toHaveAttribute(
            "src",
            `/assets/flags/${userMock.countryCode}.svg`,
        );
    });

    it("should display the about me text", () => {
        renderProfile();

        expect(screen.getByTestId("aboutMe").textContent).toBe(userMock.about);
    });

    it("should render the profile picture correctly", () => {
        renderProfile();

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userMock.userId}`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
    });

    it("should render Edit Profile button for the logged-in user's own profile", () => {
        userMock.userId = loggedInUserMock.userId;
        renderProfile();

        expect(screen.getByTestId("editProfileLink").getAttribute("href")).toBe(
            constants.PATHS.SETTINGS_PROFILE,
        );
        expect(
            screen.queryByTestId("profileStarButton"),
        ).not.toBeInTheDocument();
    });

    it("should render Star button for logged in users viewing someone else's profile", () => {
        renderProfile({ starCount: 3 });

        const starButton = screen.getByTestId("profileStarButton");
        expect(starButton).toBeInTheDocument();
        expect(starButton).toHaveTextContent("Star");
        expect(screen.getByTestId("profileStarCount")).toHaveTextContent("3");
    });

    it("should toggle star state when Star button is clicked", async () => {
        const user = userEvent.setup();
        renderProfile({ starCount: 2 });

        const starButton = screen.getByTestId("profileStarButton");
        const starCount = screen.getByTestId("profileStarCount");

        expect(starButton).toHaveTextContent("Star");
        expect(starCount).toHaveTextContent("2");

        await user.click(starButton);
        expect(addStarMock).toHaveBeenCalledWith({
            path: { starredUserId: userMock.userId },
        });

        expect(starButton).toHaveTextContent("Starred");
        expect(starCount).toHaveTextContent("3");

        await user.click(starButton);
        expect(removeStarMock).toHaveBeenCalledWith({
            path: { starredUserId: userMock.userId },
        });
        expect(starButton).toHaveTextContent("Star");
        expect(starCount).toHaveTextContent("2");
    });

    it("should render only Challenge button for guest users", () => {
        renderProfile({ isLoggedOut: true });

        expect(
            screen.getByTestId("profileChallengeButton"),
        ).toBeInTheDocument();
        expect(screen.queryByText(/Edit Profile/i)).not.toBeInTheDocument();
        expect(
            screen.queryByTestId("profileStarButton"),
        ).not.toBeInTheDocument();
    });

    it("should display the correct joined date", () => {
        renderProfile();

        const createdAtText = new Date(userMock.createdAt).toLocaleDateString(
            "en-US",
            { month: "short", day: "numeric", year: "numeric" },
        );

        expect(screen.getByTestId("profileCreatedAt")).toHaveTextContent(
            createdAtText,
        );
    });

    it("should display the correct quest points", () => {
        renderProfile({ questPoints: 123 });

        expect(screen.getByTestId("profileQuestPoints")).toHaveTextContent(
            "123",
        );
    });
});
