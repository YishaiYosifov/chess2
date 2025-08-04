import { act, render, screen } from "@testing-library/react";
import GameControlButton from "../GameControlButton";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

const MockIcon = () => <svg data-testid="mockIcon" />;

describe("GameControlButton", () => {
    let onClick: Mock;

    beforeEach(() => {
        onClick = vi.fn();
        vi.useFakeTimers({ shouldAdvanceTime: true });
    });

    it("should render icon and children", () => {
        render(<GameControlButton icon={MockIcon}>Resign</GameControlButton>);

        expect(screen.getByTestId("mockIcon")).toBeInTheDocument();
        expect(screen.getByText("Resign")).toBeInTheDocument();
        expect(
            screen.queryByTestId("gameControlCancelButton"),
        ).not.toBeInTheDocument();
    });

    it("should call onClick immediately if no confirmation is needed", async () => {
        const user = userEvent.setup();
        render(
            <GameControlButton icon={MockIcon} onClick={onClick}>
                Resign
            </GameControlButton>,
        );

        await user.click(screen.getByTestId("gameControlButton"));
        expect(onClick).toHaveBeenCalledTimes(1);
    });

    it("should enter confirmation state on first click if needsConfirmation is true", async () => {
        const user = userEvent.setup();

        render(
            <GameControlButton
                icon={MockIcon}
                onClick={onClick}
                needsConfirmation
            >
                Resign
            </GameControlButton>,
        );

        await user.click(screen.getByTestId("gameControlButton"));
        expect(onClick).not.toHaveBeenCalled();
        expect(
            screen.getByTestId("gameControlCancelButton"),
        ).toBeInTheDocument();
    });

    it("should call onClick on second confirm click if needsConfirmation is true", async () => {
        const user = userEvent.setup();

        render(
            <GameControlButton
                icon={MockIcon}
                onClick={onClick}
                needsConfirmation
            >
                Resign
            </GameControlButton>,
        );

        const button = screen.getByTestId("gameControlButton");
        await user.click(button); // triggers confirmation
        await user.click(button); // confirms

        expect(onClick).toHaveBeenCalledTimes(1);
    });

    it("should cancel confirmation state when cancel button is clicked", async () => {
        const user = userEvent.setup();

        render(
            <GameControlButton
                icon={MockIcon}
                onClick={onClick}
                needsConfirmation
            >
                Resign
            </GameControlButton>,
        );

        await user.click(screen.getByTestId("gameControlButton"));
        const cancel = screen.getByTestId("gameControlCancelButton");
        await user.click(cancel);

        expect(cancel).not.toBeInTheDocument();
        expect(onClick).not.toHaveBeenCalled();
    });

    it("should auto-cancel confirmation after 3 seconds", async () => {
        const user = userEvent.setup();

        render(
            <GameControlButton icon={MockIcon} needsConfirmation>
                Resign
            </GameControlButton>,
        );

        await user.click(screen.getByTestId("gameControlButton"));

        act(() => {
            vi.advanceTimersByTime(3000);
        });

        expect(
            screen.queryByTestId("gameControlCancelButton"),
        ).not.toBeInTheDocument();
        expect(onClick).not.toHaveBeenCalled();
    });
});
