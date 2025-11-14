import { render, screen } from "@testing-library/react";
import WinStreakObjective from "../WinStreakObjective";

describe("WinStreakObjective", () => {
    it("should render the main heading", () => {
        render(<WinStreakObjective />);
        expect(
            screen.getByRole("heading", { name: /THE OBJECTIVE:/i }),
        ).toBeInTheDocument();
    });

    it("should render the subheading and description", () => {
        render(<WinStreakObjective />);
        expect(
            screen.getByRole("heading", {
                name: /Get the longest win streak you possibly can/i,
            }),
        ).toBeInTheDocument();
        expect(
            screen.getByText(
                /Players with the highest win streak will rank higher on the leaderboard/i,
            ),
        ).toBeInTheDocument();
    });

    it("should render all ObjectiveCards with correct titles", () => {
        render(<WinStreakObjective />);
        expect(
            screen.getByRole("heading", { name: /Prizes/i }),
        ).toBeInTheDocument();
        expect(
            screen.getByRole("heading", { name: /How to Play/i }),
        ).toBeInTheDocument();
        expect(
            screen.getByRole("heading", { name: /Leaderboards/i }),
        ).toBeInTheDocument();
    });

    it("should render all ObjectiveCards with correct text", () => {
        render(<WinStreakObjective />);
        expect(
            screen.getByText(
                /\$500 total prize pool split between the top three players/i,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByText(
                /Play rated matches until the leaderboard locks, each consecutive win adds to your streak/i,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByText(
                /Your highest streak will be verified and publicly displayed on the leaderboard/i,
            ),
        ).toBeInTheDocument();
    });
});
