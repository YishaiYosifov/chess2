import { render, screen } from "@testing-library/react";

import { createUser } from "@/lib/testUtils/fakers/userFaker";
import { User } from "@/lib/apiClient";
import Profile from "../Profile";

describe("Profile", () => {
    let userMock: User;

    beforeEach(() => (userMock = createUser()));

    it("should render the profile correcetly", () => {
        render(<Profile profile={userMock} />);

        expect(screen.queryByAltText("profile picture")).toBeInTheDocument();
        expect(screen.queryByTestId("username")).toBeInTheDocument();
        expect(screen.queryByTestId("aboutMe")).toBeInTheDocument();
    });

    it("should display the username", () => {
        render(<Profile profile={userMock} />);

        expect(screen.queryByText(userMock.userName!)).toBeInTheDocument();
        const flag = screen.getByTestId("flag");
        expect(flag).toHaveAttribute(
            "src",
            `/assets/flags/${userMock.countryCode}.svg`,
        );
    });

    it("should display the about me", () => {
        render(<Profile profile={userMock} />);
        expect(screen.getByTestId("aboutMe").textContent).toBe(userMock.about);
    });

    it("should render the profile picture correctly", () => {
        render(<Profile profile={userMock} />);

        const profilePicture = screen.getByAltText("profile picture");
        const profilePictureSrc = `/assets/logo-image-temp.webp`;
        expect(profilePicture).toHaveAttribute("src", profilePictureSrc);
    });
});
