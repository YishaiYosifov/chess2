import { render } from "@testing-library/react";

import Image from "next/image";

import ProfilePicture from "@/features/profile/components/ProfilePicture";

vi.mock("next/image");

describe("ProfilePicture", () => {
    it("should render with default props", () => {
        const { queryByAltText } = render(<ProfilePicture />);
        const profilePicture = queryByAltText("profile picture");

        expect(profilePicture).toBeInTheDocument();
    });

    it("should render with custom props", () => {
        const className = "test-class";
        const username = "testuser";
        const size = 150;

        render(
            <ProfilePicture
                userId={username}
                width={size}
                height={size}
                className={className}
            />,
        );

        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                className: `aspect-square rounded-md ${className}`,
                src: "/assets/logo-image-temp.webp",
                alt: "profile picture",
                width: size,
                height: size,
            }),
            undefined,
        );
    });
});
