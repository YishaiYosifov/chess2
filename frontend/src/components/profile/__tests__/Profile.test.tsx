import { render, screen } from "@testing-library/react";

import { createUser } from "@/lib/testUtils/fakers/userFaker";
import { User } from "@/lib/apiClient/models";
import Profile from "../Profile";

describe("Profile", () => {
    let profileMock: User;

    beforeEach(() => (profileMock = createUser()));

    it("should render the profile correcetly", () => {
        render(<Profile profile={profileMock} />);

        expect(screen.queryByAltText("profile picture")).toBeInTheDocument();
        expect(screen.queryByTestId("username")).toBeInTheDocument();
        expect(screen.queryByTestId("aboutMe")).toBeInTheDocument();
    });

    it("should display the username", () => {
        render(<Profile profile={profileMock} />);

        expect(screen.queryByText(profileMock.username)).toBeInTheDocument();
        const flag = screen.getByTestId("flag");
        expect(flag).toHaveAttribute(
            "src",
            `/assets/flags/${profileMock.countryCode}.svg`,
        );
    });

    it("should display the about me", () => {
        render(<Profile profile={profileMock} />);
        expect(screen.getByTestId("aboutMe").textContent).toBe(
            profileMock.about,
        );
    });

    it("should render the profile picture correctly", () => {
        render(<Profile profile={profileMock} />);

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc = `/assets/logo-image-temp.webp?${profileMock.pfpLastChanged}`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
    });
});
