import { render, screen } from "@testing-library/react";
import GuideSide from "../GuideSide";

describe("GuideSide component", () => {
    it("should set the correct href for the Pieces and Rules links", () => {
        render(
            <GuideSide
                piecesGuideHref={"#pieces-guide"}
                rulesGuideHref={"#rules-guide"}
            />,
        );
        const piecesLink = screen.getByText("Pieces");
        const rulesLink = screen.getByText("Rules");

        expect(piecesLink.getAttribute("href")).toBe("#pieces-guide");
        expect(rulesLink.getAttribute("href")).toBe("#rules-guide");
    });
});
