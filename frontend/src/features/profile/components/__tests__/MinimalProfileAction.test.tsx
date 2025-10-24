import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import { createFakeMinimalProfile } from "@/lib/testUtils/fakers/minimalProfileFaker";
import MinimalProfileAction from "../MinimalProfileAction";
import { MinimalProfile } from "@/lib/apiClient";

describe("MinimalProfileAction", () => {
    let profileMock: MinimalProfile;
    let activate: Mock;
    let deactivate: Mock;

    beforeEach(() => {
        profileMock = createFakeMinimalProfile();
        activate = vi.fn().mockResolvedValue(undefined);
        deactivate = vi.fn().mockResolvedValue(undefined);
    });

    it("should render the minimal profile inside the view", () => {
        render(
            <MinimalProfileAction
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        expect(
            screen.getByTestId("minimalProfileRowUsername"),
        ).toHaveTextContent(profileMock.userName);
    });

    it("should render button label correctly for active state", () => {
        render(
            <MinimalProfileAction
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        expect(
            screen.getByTestId("minimalProfileActionRowToggle"),
        ).toHaveTextContent("Active");
    });

    it("should toggle between activate and deactivate", async () => {
        const user = userEvent.setup();

        render(
            <MinimalProfileAction
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        const button = screen.getByTestId("minimalProfileActionRowToggle");

        await user.click(button);
        expect(deactivate).toHaveBeenCalledTimes(1);

        await user.click(button);
        expect(activate).toHaveBeenCalledTimes(1);
    });

    it("should render button icon correctly", async () => {
        const user = userEvent.setup();

        const ActiveIcon = () => <span data-testid="activeIcon" />;
        const InactiveIcon = () => <span data-testid="inactiveIcon" />;

        render(
            <MinimalProfileAction
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

        const button = screen.getByTestId("minimalProfileActionRowToggle");
        await user.click(button);

        expect(screen.getByTestId("inactiveIcon")).toBeInTheDocument();
    });

    it("should prevent double clicks while loading", async () => {
        const user = userEvent.setup();
        let resolveDeactivate: () => void;

        deactivate = vi.fn(
            () =>
                new Promise<void>((resolve) => {
                    resolveDeactivate = resolve;
                }),
        );

        render(
            <MinimalProfileAction
                index={0}
                profile={profileMock}
                activate={activate}
                deactivate={deactivate}
                buttonLabel={(active) => (active ? "Active" : "Inactive")}
            />,
        );

        const button = screen.getByTestId("minimalProfileActionRowToggle");

        const clickPromise = user.click(button);
        await user.click(button);

        expect(deactivate).toHaveBeenCalledTimes(1);

        resolveDeactivate!();
        await clickPromise;
        expect(deactivate).toHaveBeenCalledTimes(1);
    });
});
