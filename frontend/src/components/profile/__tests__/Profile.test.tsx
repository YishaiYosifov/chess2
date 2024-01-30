import { render, screen } from "@testing-library/react";

import { profileMock } from "@/mockUtils/profileMock";
import type { TypedCountries } from "@/lib/types";
import countries from "@/data/countries.json";

import Profile from "../Profile";

describe("Profile", () => {
    it("should render the profile correcetly", () => {
        render(<Profile profile={profileMock} />);

        expect(screen.getByAltText("profile picture")).toBeInTheDocument();
        expect(screen.getByTestId("aboutArea")).toBeInTheDocument();
    });

    it("should display the username", () => {
        render(<Profile profile={profileMock} />);
        expect(
            screen.getByText(
                `${
                    (countries as TypedCountries)[profileMock.countryAlpha3]
                        .flag
                } ${profileMock.username}`
            )
        ).toBeInTheDocument();
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
            `${profileMock.username}/profile-picture?${
                profileMock.pfpLastChanged.valueOf() / 1000
            }`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
        expect(profilePicture).toHaveAttribute("width", "250");
        expect(profilePicture).toHaveAttribute("height", "250");
    });
});
