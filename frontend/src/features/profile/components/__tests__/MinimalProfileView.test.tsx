import { render, screen, within } from "@testing-library/react";

import { createFakeMinimalProfile } from "@/lib/testUtils/fakers/minimalProfileFaker";
import MinimalProfileView from "../MinimalProfileView";
import { MinimalProfile } from "@/lib/apiClient";

describe("MinimalProfileView", () => {
    let profileMock: MinimalProfile;

    beforeEach(() => {
        profileMock = createFakeMinimalProfile();
    });

    it("should render profile with tooltip", async () => {
        render(<MinimalProfileView profile={profileMock} />);

        const tooltip = screen.getByTestId("userProfileTooltipChildren");
        expect(
            within(tooltip).getByTestId("minimalProfileRowUsername"),
        ).toBeInTheDocument();
    });

    it("should render profile username", () => {
        render(<MinimalProfileView profile={profileMock} />);

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(profileMock.userName);
    });

    it("should alternate background color based on index", () => {
        const { rerender } = render(
            <MinimalProfileView profile={profileMock} index={0} />,
        );
        expect(screen.getByTestId("minimalProfileRow")).toHaveClass(
            "bg-white/5",
        );

        rerender(<MinimalProfileView profile={profileMock} index={1} />);
        expect(screen.getByTestId("minimalProfileRow")).toHaveClass(
            "bg-white/15",
        );
    });

    it("should render children if provided", () => {
        render(
            <MinimalProfileView profile={profileMock}>
                <div data-testid="child">Child</div>
            </MinimalProfileView>,
        );

        expect(screen.getByTestId("child")).toBeInTheDocument();
    });
});
