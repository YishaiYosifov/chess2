import { render, screen } from "@testing-library/react";

import {
    createFakePrivateUser,
    createFakeUser,
} from "@/lib/testUtils/fakers/userFaker";
import { addStar, PrivateUser, PublicUser, removeStar } from "@/lib/apiClient";
import Profile from "../Profile";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import userEvent from "@testing-library/user-event";

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

    it("should render the profile correctly", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={5}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        expect(screen.getByAltText("profile picture")).toBeInTheDocument();
        expect(screen.getByTestId("username")).toBeInTheDocument();
        expect(screen.getByTestId("aboutMe")).toBeInTheDocument();
        expect(screen.getByTestId("profileStarCount")).toHaveTextContent("5");
        expect(screen.getByText(/Joined/i)).toBeInTheDocument();
    });

    it("should display the username and flag correctly", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        expect(screen.getByText(userMock.userName!)).toBeInTheDocument();
        const flag = screen.getByTestId("flag");
        expect(flag).toHaveAttribute(
            "src",
            `/assets/flags/${userMock.countryCode}.svg`,
        );
    });

    it("should display the about me text", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        expect(screen.getByTestId("aboutMe").textContent).toBe(userMock.about);
    });

    it("should render the profile picture correctly", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userMock.userId}`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
    });

    it("should render Edit Profile button for the logged-in user's own profile", () => {
        userMock.userId = loggedInUserMock.userId;

        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        expect(screen.getByText(/Edit Profile/i)).toBeInTheDocument();
    });

    it("should render Star button for logged in users viewing someone else's profile", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={3}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        const starButton = screen.getByTestId("profileStarButton");
        expect(starButton).toBeInTheDocument();
        expect(starButton).toHaveTextContent("Star");
        expect(screen.getByTestId("profileStarCount")).toHaveTextContent("3");
    });

    it("should toggle star state when Star button is clicked", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={2}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        const starButton = screen.getByTestId("profileStarButton");
        const starCount = screen.getByTestId("profileStarCount");

        expect(starButton).toHaveTextContent("Star");
        expect(starCount).toHaveTextContent("2");

        await user.click(starButton);
        expect(addStarMock).toHaveBeenCalledWith({
            path: { starredUserId: userMock.userId },
        });

        expect(starButton).toHaveTextContent("Unstar");
        expect(starCount).toHaveTextContent("3");

        await user.click(starButton);
        expect(removeStarMock).toHaveBeenCalledWith({
            path: { starredUserId: userMock.userId },
        });
        expect(starButton).toHaveTextContent("Star");
        expect(starCount).toHaveTextContent("2");
    });

    it("should render only Challenge button for guest users", () => {
        render(
            <SessionProvider user={null} fetchAttempted>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("profileChallengeButton"),
        ).toBeInTheDocument();
        expect(screen.queryByText(/Edit Profile/i)).not.toBeInTheDocument();
        expect(
            screen.queryByTestId("profileStarButton"),
        ).not.toBeInTheDocument();
    });

    it("should display the correct joined date", () => {
        render(
            <SessionProvider user={loggedInUserMock}>
                <Profile
                    profile={userMock}
                    initialStarCount={0}
                    initialHasStarred={false}
                />
            </SessionProvider>,
        );

        const createdAtText = new Date(userMock.createdAt).toLocaleDateString(
            "en-US",
            { month: "short", day: "numeric", year: "numeric" },
        );

        expect(screen.getByTestId("profileCreatedAt")).toHaveTextContent(
            createdAtText,
        );
    });
});
