import { render } from "@testing-library/react";

import ProfilePicture from "../ProfilePicture";
import Image from "next/image";

describe("ProfilePicture", () => {
    it("should render with default props", () => {
        const { getByAltText } = render(<ProfilePicture />);
        const profilePicture = getByAltText("profile picture");

        expect(profilePicture).toBeInTheDocument();
    });

    it("should render with custom props", () => {
        const lastChanged = "2023-01-01";
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
                src: `${process.env.NEXT_PUBLIC_API_URL}/profile/${username}/profile-picture?${lastChanged}`,
                alt: "profile picture",
                width: size,
                height: size,
            },
            {}
        );
    });
});
