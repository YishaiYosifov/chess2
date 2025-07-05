import { render } from "@testing-library/react";

import Image from "next/image";

import ProfilePicture from "@/features/profile/components/ProfilePicture";

describe("ProfilePicture", () => {
    it("should render with default props", () => {
        const { queryByAltText } = render(<ProfilePicture />);
        const profilePicture = queryByAltText("profile picture");

        expect(profilePicture).toBeInTheDocument();
    });

    it("should render with custom props", () => {
        const lastChanged = new Date("2023-01-01");
        const className = "test-class";
        const username = "testuser";
        const size = 150;

        render(
            <ProfilePicture
                userId={username}
                width={size}
                height={size}
                lastChanged={lastChanged.valueOf()}
                className={className}
            />,
        );

        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                className: className,
                src:
                    `${process.env.API_URL}/profile/${username}` +
                    `/profile-picture?${lastChanged.valueOf() / 1000}`,
                alt: "profile picture",
                width: size,
                height: size,
            }),
            {},
        );
    });
});
