import { render, screen } from "@testing-library/react";
import WinStreakRules from "../WinStreakRules";

describe("WinStreakRules", () => {
    it("should render the main heading", () => {
        render(<WinStreakRules />);
        expect(
            screen.getByRole("heading", { name: /THE RULES:/i }),
        ).toBeInTheDocument();
    });

    it('should render the "CONTRIBUTES TO STREAK" section', () => {
        render(<WinStreakRules />);
        expect(
            screen.getByRole("heading", { name: /CONTRIBUTES TO STREAK:/i }),
        ).toBeInTheDocument();
        expect(
            screen.getByText(/Rated games through matchmaking only/i),
        ).toBeInTheDocument();
    });

    it('should render the "DOES NOT CONTRIBUTE TO STREAK" section with list items', () => {
        render(<WinStreakRules />);
        expect(
            screen.getByRole("heading", {
                name: /DOES NOT CONTRIBUTE TO STREAK:/i,
            }),
        ).toBeInTheDocument();

        const listItems = [
            "Rematches",
            "Direct challenges",
            "Aborted games",
            "Draws",
            "Casual games",
        ];

        listItems.forEach((item) => {
            expect(screen.getByText(item)).toBeInTheDocument();
        });

        expect(
            screen.getByText(
                /Games in these categories won't impact your streak, so losing one won't break your run/i,
            ),
        ).toBeInTheDocument();
    });

    it('should render the "DO NOT" section with list items', () => {
        render(<WinStreakRules />);
        expect(
            screen.getByRole("heading", { name: /DO NOT:/i }),
        ).toBeInTheDocument();

        const listItems = [
            "Abuse accounts or share them",
            "Collude or manipulate matches",
            "Use cheats, bots, or exploits",
        ];

        listItems.forEach((item) => {
            expect(screen.getByText(item)).toBeInTheDocument();
        });

        expect(
            screen.getByText(
                /We can see which games contributed to your streak. Violations will lead to disqualification/i,
            ),
        ).toBeInTheDocument();
    });
});
