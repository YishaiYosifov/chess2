import { render, screen } from "@testing-library/react";
import NewPiecesGuide from "../NewPiecesGuide";

vi.mock("next/image");

describe("NewPiecesGuide", () => {
    it("should render the main heading", () => {
        render(<NewPiecesGuide />);
        expect(
            screen.getByRole("heading", { name: /New Pieces/i }),
        ).toBeInTheDocument();
    });

    it("should render all guide cards with correct titles", () => {
        render(<NewPiecesGuide />);

        const titles = [
            "Knook",
            "Underage Pawn",
            "Checker",
            "Traitor Rook (Neutral Piece)",
            "Antiqueen",
        ];

        const titleElements = screen.getAllByTestId("guideCardTitle");
        expect(titleElements).toHaveLength(titles.length);

        titles.forEach((title, i) => {
            expect(titleElements[i]).toHaveTextContent(title);
        });
    });

    it("should attach the id", () => {
        const { container } = render(<NewPiecesGuide id="pieces-guide" />);
        expect(container.firstChild).toHaveAttribute("id", "pieces-guide");
    });
});
