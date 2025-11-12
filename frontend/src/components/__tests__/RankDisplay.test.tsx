import { render, screen } from "@testing-library/react";
import RankDisplay from "../RankDisplay";

describe("RankDisplay", () => {
    it("should display the correct rank", () => {
        render(<RankDisplay rank={69} totalPlayers={100} />);

        expect(screen.getByTestId("rankDisplayNumber")).toHaveTextContent(
            "#69",
        );
    });

    it("should calculate percentile correctly for top rank", () => {
        render(<RankDisplay rank={1} totalPlayers={100} />);

        expect(screen.getByTestId("rankDisplayPercentile")).toHaveTextContent(
            "That's top 99.0%!",
        );
    });

    it("should calculate percentile correctly for last rank", () => {
        render(<RankDisplay rank={100} totalPlayers={100} />);
        expect(screen.getByTestId("rankDisplayPercentile")).toHaveTextContent(
            "That's top 0.0%!",
        );
    });

    it("should handle 0 players", () => {
        render(<RankDisplay rank={1} totalPlayers={0} />);

        expect(screen.getByTestId("rankDisplayNumber")).toHaveTextContent("-");
        expect(screen.getByTestId("rankDisplayPercentile")).toHaveTextContent(
            "No players yet!",
        );
    });
});
