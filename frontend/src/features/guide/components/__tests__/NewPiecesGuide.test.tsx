import { render, screen } from "@testing-library/react";
import NewPiecesGuide from "../NewPiecesGuide";

vi.mock("next/image");

describe("NewPiecesGuide", () => {
    it("should render all guide cards with correct titles", () => {
        render(<NewPiecesGuide />);

        const titles = [
            "Knook",
            "Checker",
            "Underage Pawn",
            "Traitor Rook (Neutral Piece)",
            "Antiqueen",
        ];

        const titleElements = screen.getAllByTestId("guideCardTitle");
        expect(titleElements).toHaveLength(titles.length);

        titles.forEach((title, i) => {
            expect(titleElements[i]).toHaveTextContent(title);
        });
    });
});
