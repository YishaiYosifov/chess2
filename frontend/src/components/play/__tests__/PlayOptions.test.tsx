import { render, screen } from "@testing-library/react";
import userEvent, { UserEvent } from "@testing-library/user-event";

import { TIME_CONTROLS } from "@/lib/constants";
import PlayOptions from "../PlayOptions";
import { Variant } from "@/client";
import { gameRequestApi } from "@/lib/apis";
import { Mock } from "vitest";
import { mockRouter } from "@/mockUtils/mockRouter";

vi.mock("@/lib/constants", async (importOriginal) => ({
    ...(await importOriginal<typeof import("@/lib/constants")>()),
    TIME_CONTROLS: [
        {
            type: "bullet",
            timeControl: 60,
            increment: 0,
        },
        { type: "blitz", timeControl: 180, increment: 0 },
        { type: "rapid", timeControl: 900, increment: 10 },
    ],
    Variant: { FogOfWar: "fog of war", Anarchy: "anarchy", Chss: "chss" },
}));

vi.mock("@/lib/apis");

describe("PlayOptions", () => {
    async function joinPoolFirstButton(user: UserEvent, index: number = 0) {
        const timeControlButton =
            screen.getAllByTestId("timeControlButton")[index];
        await user.click(timeControlButton);

        return { timeControl: TIME_CONTROLS[index], timeControlButton };
    }

    it("should render without crashing", () => {
        render(<PlayOptions />);
        expect(screen.queryByText("Variant")).toBeInTheDocument();
        expect(screen.queryByText("Time Control")).toBeInTheDocument();
    });

    it("should correctly render the time controls", () => {
        render(<PlayOptions />);

        const timeControlButtons = screen.getAllByTestId("timeControlButton");
        TIME_CONTROLS.forEach((timeControl, i) => {
            expect(timeControlButtons[i].textContent).toBe(
                `${timeControl.timeControl / 60} + ${timeControl.increment}${
                    timeControl.type
                }`
            );
        });
    });

    it("should correctly render the variant buttons", () => {
        render(<PlayOptions />);

        const variantButtons = screen.getAllByTestId("variantButton");
        Object.values(Variant).forEach((variant, i) => {
            expect(variantButtons[i].textContent).toBe(variant);
        });
    });

    it("should allow variant change", async () => {
        const user = userEvent.setup();
        render(<PlayOptions />);

        const variantButton = screen.getByText(Object.values(Variant)[0]);
        await user.click(variantButton);

        expect(variantButton).toHaveClass("selected-variant");
    });

    it("should attempt to join the pool on time control selection", async () => {
        const user = userEvent.setup();
        render(<PlayOptions />);

        const { timeControl, timeControlButton } = await joinPoolFirstButton(
            user
        );

        expect(timeControlButton).toHaveClass("selected-time-control");
        expect(gameRequestApi.startPoolGameRaw).toHaveBeenCalledOnce();
        expect(gameRequestApi.startPoolGameRaw).toHaveBeenCalledWith({
            gameSettings: {
                variant: Variant.Anarchy,
                timeControl: timeControl.timeControl,
                increment: timeControl.increment,
            },
        });

        expect(gameRequestApi.cancel).not.toHaveBeenCalled();
    });

    it("should redirect when the game starts", async () => {
        const { push } = mockRouter();

        const token = "test-token";
        const startPoolGameRawMock = gameRequestApi.startPoolGameRaw as Mock;
        startPoolGameRawMock.mockResolvedValue({
            raw: { status: 200 },
            value: () => token,
        });

        const user = userEvent.setup();
        render(<PlayOptions />);
        await joinPoolFirstButton(user);

        expect(push).toHaveBeenCalledOnce();
        expect(push).toHaveBeenCalledWith(`/game/${token}`);
    });

    it("should cancel outgoing requests when changing time control", async () => {
        const user = userEvent.setup();
        render(<PlayOptions />);

        await joinPoolFirstButton(user);
        await joinPoolFirstButton(user);

        expect(gameRequestApi.cancel).toHaveBeenCalled();
    });
});
