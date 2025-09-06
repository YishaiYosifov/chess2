import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { RelationProfileRow } from "../RelationProfileRow";
import { createFakeMinimalProfile } from "@/lib/testUtils/fakers/minimalProfileFaker";
import { MinimalProfile } from "@/lib/apiClient";

describe("RelationProfileRow", () => {
    let profileMock: MinimalProfile;

    const activate = vi.fn();
    const deactivate = vi.fn();

    beforeEach(() => {
        profileMock = createFakeMinimalProfile();
    });

    it("should render the profile name and link correctly", async () => {
        render(
            <RelationProfileRow
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        const link = screen.getByTestId("relationProfileRowLink");
        expect(link).toHaveAttribute(
            "href",
            `/profile/${profileMock.userName}`,
        );
        expect(
            screen.getByTestId("relationProfileRowUsername"),
        ).toHaveTextContent(profileMock.userName);
    });

    it("should toggle when button is clicked", async () => {
        const user = userEvent.setup();
        render(
            <RelationProfileRow
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        const button = screen.getByTestId("relationProfileRowToggle");

        await user.click(button);
        expect(deactivate).toHaveBeenCalledTimes(1);

        await user.click(button);
        expect(activate).toHaveBeenCalledTimes(1);
    });

    it("should show the button icon correctly based on active state", async () => {
        const user = userEvent.setup();

        const ActiveIcon = () => <span data-testid="activeIcon" />;
        const InactiveIcon = () => <span data-testid="inactiveIcon" />;

        render(
            <RelationProfileRow
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
                buttonIcon={(active) =>
                    active ? <ActiveIcon /> : <InactiveIcon />
                }
            />,
        );

        expect(screen.getByTestId("activeIcon")).toBeInTheDocument();
        expect(screen.queryByTestId("inactiveIcon")).not.toBeInTheDocument();

        const button = screen.getByTestId("relationProfileRowToggle");
        await user.click(button);

        expect(screen.getByTestId("inactiveIcon")).toBeInTheDocument();
        expect(screen.queryByTestId("activeIcon")).not.toBeInTheDocument();
    });

    it("should prevent double clicks while loading", async () => {
        const user = userEvent.setup();
        let resolveDeactivate: () => void;
        const deactivate = vi.fn(
            () =>
                new Promise<void>((resolve) => {
                    resolveDeactivate = resolve;
                }),
        );
        const activate = vi.fn().mockResolvedValue(undefined);

        render(
            <RelationProfileRow
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        const button = screen.getByTestId("relationProfileRowToggle");

        const clickPromise = user.click(button);
        await user.click(button);

        expect(deactivate).toHaveBeenCalledTimes(1);

        resolveDeactivate!();
        await clickPromise;
        expect(deactivate).toHaveBeenCalledTimes(1);
    });
});
