import { render } from "@testing-library/react";

import Image from "next/image";

import ProfilePicture from "../ProfilePicture";

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
                username={username}
                width={size}
                height={size}
                lastChanged={lastChanged}
                className={className}
            />
        );

        expect(Image).toHaveBeenCalledWith(
            {
                className: className,
                src:
                    `${process.env.NEXT_PUBLIC_API_URL}/profile/${username}` +
                    `/profile-picture?${lastChanged.valueOf() / 1000}`,
                alt: "profile picture",
                width: size,
                height: size,
            },
            {}
        );
    });
});
