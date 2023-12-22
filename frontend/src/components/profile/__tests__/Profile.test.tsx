import { render, screen } from "@testing-library/react";

import { PublicProfile } from "@/lib/types";

import Profile from "../Profile";

const mockProfile: PublicProfile = {
    userId: 1,
    username: "testuser",
    about: "test about",

    country: "ISR",
    pfpLastChanged: "2023-01-01T12:00:00Z",
};

describe("Profile", () => {
    it("should render the profile correcetly", () => {
        render(<Profile profile={mockProfile} />);

        expect(screen.getByAltText("profile picture")).toBeInTheDocument();
        expect(screen.getByTestId("countryImage")).toBeInTheDocument();
        expect(screen.getByTestId("aboutArea")).toBeInTheDocument();
    });

    it("should display the username", () => {
        render(<Profile profile={mockProfile} />);
        expect(screen.getByText(mockProfile.username)).toBeInTheDocument();
    });

    it("should display the about me", () => {
        render(<Profile profile={mockProfile} />);
        expect(screen.getByTestId("aboutArea").textContent).toBe(
            mockProfile.about
        );
    });

    it("should render the profile picture correctly", () => {
        render(<Profile profile={mockProfile} />);

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc =
            `${process.env.NEXT_PUBLIC_API_URL}/profile/` +
            `${mockProfile.username}/profile-picture?${mockProfile.pfpLastChanged}`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
        expect(profilePicture).toHaveAttribute("width", "250");
        expect(profilePicture).toHaveAttribute("height", "250");
    });
});
