import { render, screen } from "@testing-library/react";
import TimeControlButton from "../TimeControlButton";
import userEvent from "@testing-library/user-event";
import { TimeControlSettings } from "@/lib/apiClient";

describe("TimeControlButton", () => {
    it("should render the formatted time control and type", () => {
        render(
            <TimeControlButton
                timeControl={{ baseSeconds: 300, incrementSeconds: 3 }}
                label="Rapid"
            />,
        );

        expect(screen.getByText("5 + 3")).toBeInTheDocument();
        expect(screen.getByText("Rapid")).toBeInTheDocument();
    });

    it("should show 'Most Popular' label and apply border if isMostPopular is true", () => {
        render(
            <TimeControlButton
                timeControl={{ baseSeconds: 180, incrementSeconds: 2 }}
                label="Blitz"
                isMostPopular
            />,
        );

        expect(screen.getByText("Most Popular")).toBeInTheDocument();

        const button = screen.getByRole("button");
        expect(button.className).toMatch(/border-amber-300/);
    });

    it("should blur the component if isSeeking is true", () => {
        const { container } = render(
            <TimeControlButton
                timeControl={{ baseSeconds: 600, incrementSeconds: 5 }}
                label="Classic"
                isSeeking
            />,
        );

        const wrapperDiv = container.querySelector("div");
        expect(wrapperDiv?.className).toMatch(/blur-sm/);
    });

    it("should call onClick with baseMinutes and increment when clicked", async () => {
        const user = userEvent.setup();
        const handleClick = vi.fn();
        const timeControl: TimeControlSettings = {
            baseSeconds: 60,
            incrementSeconds: 5,
        };

        render(
            <TimeControlButton
                timeControl={timeControl}
                label="Bullet"
                onClick={handleClick}
            />,
        );

        const button = screen.getByRole("button");
        await user.click(button);

        expect(handleClick).toHaveBeenCalledWith(timeControl);
    });
});
