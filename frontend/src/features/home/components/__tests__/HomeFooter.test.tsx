import { render, screen, within } from "@testing-library/react";
import HomeFooter from "../HomeFooter";
import constants from "@/lib/constants";

describe("HomeFooter", () => {
    it("should render footer section with About Anarchy Chess text", () => {
        render(<HomeFooter />);

        const footer = screen.getByRole("contentinfo");
        expect(footer).toBeInTheDocument();

        const about = within(footer).getByRole("heading", {
            name: /About Anarchy Chess/i,
        });
        expect(about).toBeInTheDocument();

        const aboutText = screen.getByText(
            /Anarchy Chess is my reimagining of chess/i,
        );
        expect(aboutText).toBeInTheDocument();

        const redditLink = screen.getByRole("link", {
            name: /r\/AnarchyChess/i,
        });
        expect(redditLink).toHaveAttribute(
            "href",
            "https://reddit.com/r/anarchychess",
        );
    });

    it("should render the Quick Links section with correct navigation links", () => {
        render(<HomeFooter />);

        const quickLinksHeading = screen.getByRole("heading", {
            name: /Quick Links/i,
        });
        expect(quickLinksHeading).toBeInTheDocument();

        const quickLinksSection = quickLinksHeading.closest("div");
        expect(quickLinksSection).toBeInTheDocument();

        const links = within(quickLinksSection!).getAllByRole("link");
        expect(links.map((l) => l.textContent)).toEqual([
            "Play Now",
            "Donate",
            "Daily Quests",
            "Guide",
            "Source Code",
        ]);

        const [play, donate, quests, guide, sourceCode] = links;

        expect(play).toHaveAttribute("href", constants.PATHS.PLAY);
        expect(donate).toHaveAttribute("href", constants.PATHS.DONATE);
        expect(quests).toHaveAttribute("href", constants.PATHS.QUESTS);
        expect(guide).toHaveAttribute("href", constants.PATHS.GUIDE);
        expect(sourceCode).toHaveAttribute("href", constants.PATHS.GITHUB);
    });

    it("should render the Follow Us section with social links", () => {
        render(<HomeFooter />);

        const followHeading = screen.getByRole("heading", {
            name: /Follow Us/i,
        });
        expect(followHeading).toBeInTheDocument();

        const followSection = followHeading.closest("div");
        expect(followSection).toBeInTheDocument();

        const socialLinks = within(followSection!).getAllByRole("link");
        expect(socialLinks.map((l) => l.textContent)).toEqual([
            "Discord",
            "YouTube",
        ]);

        const [discord, youtube] = socialLinks;

        expect(discord).toHaveAttribute("href", constants.PATHS.DISCORD);
        expect(youtube).toHaveAttribute("href", constants.PATHS.YOUTUBE);
    });
});
