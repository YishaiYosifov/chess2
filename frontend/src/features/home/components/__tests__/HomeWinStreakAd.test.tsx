import { render, screen } from "@testing-library/react";

import thumbnail from "@public/assets/win-streak/thumbnail.png";
import HomeWinStreakAd from "../HomeWinStreakAd";
import constants from "@/lib/constants";
import Image from "next/image";

vi.mock("next/image");

describe("HomeWinStreakAd", () => {
    it("should render the main heading", () => {
        render(<HomeWinStreakAd />);
        expect(
            screen.getByRole("heading", { name: /how far can you go/i }),
        ).toBeInTheDocument();
    });

    it("should render the subtitle text", () => {
        render(<HomeWinStreakAd />);
        expect(
            screen.getByText(
                /build the longest win streak and claim the cash prize/i,
            ),
        ).toBeInTheDocument();
    });

    it("should render the WinStreakLeaderboardCountdown component", () => {
        render(<HomeWinStreakAd />);
        expect(
            screen.getByTestId("winStreakLeaderboardCountdown"),
        ).toBeInTheDocument();
    });

    it("should render the JOIN NOW button", () => {
        render(<HomeWinStreakAd />);
        const button = screen.getByRole("button", { name: /join now/i });
        expect(button).toBeInTheDocument();
    });

    it("should wrap the button inside a link to the WIN_STREAK path", () => {
        render(<HomeWinStreakAd />);
        const link = screen.getByRole("link", { name: /join now/i });
        expect(link).toHaveAttribute("href", constants.PATHS.WIN_STREAK);
    });

    it("should render the challenge thumbnail image with correct alt text", () => {
        render(<HomeWinStreakAd />);

        const img = screen.getByAltText("challenge thumbnail");
        expect(img).toBeInTheDocument();
        expect(Image).toHaveBeenCalledWith(
            expect.objectContaining({
                alt: "challenge thumbnail",
                width: 400,
                height: 225,
                src: thumbnail,
            }),
            undefined,
        );
    });
});
