import { render, screen } from "@testing-library/react";
import NewRulesGuide from "../NewRulesGuide";

vi.mock("next/image");

describe("NewRulesGuide", () => {
    it("should render the main heading", () => {
        render(<NewRulesGuide />);
        expect(
            screen.getByRole("heading", { name: /New Rules/i }),
        ).toBeInTheDocument();
    });

    it("should render all guide cards with correct titles", () => {
        render(<NewRulesGuide />);

        const titles = [
            "King Capture",
            "King Touch = Draw",
            "Self-Bishop Castle Capture",
            "Forced En Passant",
            "Long Passant",
            "Il Vaticano",
            "Omnipotent Pawn",
            "Vertical Castling",
            "Knooklear Fusion",
            "Queen Beta Decay",
        ];

        const titleElements = screen.getAllByTestId("guideCardTitle");
        expect(titleElements).toHaveLength(titles.length);

        titles.forEach((title, i) => {
            expect(titleElements[i]).toHaveTextContent(title);
        });
    });
});
