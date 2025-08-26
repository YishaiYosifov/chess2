import { render } from "@testing-library/react";

import Image from "next/image";

import ProfilePicture from "@/features/profile/components/ProfilePicture";

vi.mock("next/image");

describe("ProfilePicture", () => {
    it("should render with default props", () => {
        const { queryByAltText } = render(<ProfilePicture userId="test" />);
        const profilePicture = queryByAltText("profile picture");

        expect(profilePicture).toBeInTheDocument();
    });

    it("should render with custom props", () => {
        const className = "test-class";
        const userId = "testuser";
        const size = 150;

        render(
            <ProfilePicture
                userId={userId}
                width={size}
                height={size}
                className={className}
            />,
        );

        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                className: `aspect-square rounded-md ${className}`,
                src: `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`,
                alt: "profile picture",
                width: size,
                height: size,
                unoptimized: true,
            }),
            undefined,
        );
    });
});
