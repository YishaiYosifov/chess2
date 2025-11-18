import { render, screen } from "@testing-library/react";
import constants from "@/lib/constants";
import HomeHero from "../HomeHero";

vi.mock("next/image");

describe("HomeHero", () => {
    it("renders hero section with correct text", () => {
        render(<HomeHero />);

        const heroTitle = screen.getByTestId("homeHeroBanner");
        expect(heroTitle).toHaveTextContent("Welcome To");
        expect(heroTitle).toHaveTextContent("CHESS");
        expect(heroTitle).toHaveTextContent("The Anarchy Update");
    });

    it("should render the knook image", () => {
        render(<HomeHero />);
        const image = screen.getByAltText("knook");
        expect(image).toBeInTheDocument();
    });

    it("should render the main 'PLAY ANARCHY CHESS!' card with button", () => {
        render(<HomeHero />);
        expect(screen.getByText("PLAY ANARCHY CHESS!")).toBeInTheDocument();
        expect(
            screen.getByText(/stupidest chess variant/i),
        ).toBeInTheDocument();

        const button = screen.getByRole("button", { name: /PLAY NOW/i });
        expect(button).toBeInTheDocument();

        const link = screen.getByRole("link", { name: /PLAY NOW/i });
        expect(link).toHaveAttribute("href", constants.PATHS.PLAY);
    });

    it("should render the 'DAILY QUEST' card", () => {
        render(<HomeHero />);
        expect(screen.getByText("DAILY QUEST")).toBeInTheDocument();
        expect(screen.getByText(/new challenge/i)).toBeInTheDocument();

        const questLink = screen.getByRole("link", { name: /PLAY QUEST/i });
        expect(questLink).toHaveAttribute("href", constants.PATHS.QUESTS);
    });

    it("should render the 'NEW RULES' card", () => {
        render(<HomeHero />);
        expect(screen.getByText("NEW RULES")).toBeInTheDocument();
        expect(
            screen.getByText(/Rules\? In this economy\?/i),
        ).toBeInTheDocument();

        const guideLink = screen.getByRole("link", { name: /VIEW GUIDE/i });
        expect(guideLink).toHaveAttribute("href", constants.PATHS.GUIDE);
    });
});
