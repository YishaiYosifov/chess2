import { render, screen } from "@testing-library/react";

import { profileMock } from "@/mocks/mocks";

import Profile from "../Profile";

describe("Profile", () => {
    it("should render the profile correcetly", () => {
        render(<Profile profile={profileMock} />);

        expect(screen.getByAltText("profile picture")).toBeInTheDocument();
        expect(screen.getByTestId("countryImage")).toBeInTheDocument();
        expect(screen.getByTestId("aboutArea")).toBeInTheDocument();
    });

    it("should display the username", () => {
        render(<Profile profile={profileMock} />);
        expect(screen.getByText(profileMock.username)).toBeInTheDocument();
    });

    it("should display the about me", () => {
        render(<Profile profile={profileMock} />);
        expect(screen.getByTestId("aboutArea").textContent).toBe(
            profileMock.about
        );
    });

    it("should render the profile picture correctly", () => {
        render(<Profile profile={profileMock} />);

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc =
            `${process.env.NEXT_PUBLIC_API_URL}/profile/` +
            `${profileMock.username}/profile-picture?${profileMock.pfpLastChanged}`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
        expect(profilePicture).toHaveAttribute("width", "250");
        expect(profilePicture).toHaveAttribute("height", "250");
    });
});
