import { render, screen } from "@testing-library/react";
import HomeIntroSection from "../HomeIntroSection";
import constants from "@/lib/constants";
import homePageReplay from "@public/data/homePageReplay.json";
import StaticChessboard from "@/features/chessboard/components/StaticChessboard";

vi.mock("@/features/chessboard/components/StaticChessboard");

describe("HomeIntroSection", () => {
    it("should render the section with the correct heading and paragraphs", () => {
        render(<HomeIntroSection />);

        const heading = screen.getByRole("heading", {
            name: /Discover the Madness of Anarchy Chess/i,
        });
        expect(heading).toBeInTheDocument();

        expect(
            screen.getByText(/Don'?t worry, no one knows what'?s going on/i),
        ).toBeInTheDocument();

        const coolGameText = screen.getByText(/look at this cool game/i);
        expect(coolGameText).toBeInTheDocument();

        const pogImage = screen.getByAltText("pog");
        expect(pogImage).toBeInTheDocument();
    });

    it("should render a PLAY NOW button linking to the play page", () => {
        render(<HomeIntroSection />);

        const button = screen.getByRole("button", { name: /PLAY NOW/i });
        expect(button).toBeInTheDocument();

        const link = screen.getByRole("link", { name: /PLAY NOW/i });
        expect(link).toHaveAttribute("href", constants.PATHS.PLAY);
    });

    it("should render the StaticChessboard with replays", () => {
        render(<HomeIntroSection />);

        expect(StaticChessboard).toHaveBeenCalledWith(
            expect.objectContaining({
                replays: homePageReplay,
                canDrag: false,
            }),
            undefined,
        );
    });
});
