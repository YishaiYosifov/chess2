import { render, screen } from "@testing-library/react";
import Image from "next/image";

import thumbnail from "@public/assets/win-streak/thumbnail.png";
import WinStreakHeader from "../WinStreakHeader";

vi.mock("next/image");

describe("WinStreakHeader", () => {
    it("should render the main heading", () => {
        render(<WinStreakHeader />);
        expect(
            screen.getByRole("heading", {
                name: /The Anarchy Win-Streak Challenge/i,
            }),
        ).toBeInTheDocument();
    });

    it("should render the description paragraphs", () => {
        render(<WinStreakHeader />);
        expect(
            screen.getByText(
                /In celebration of the launch of this website, we're inviting all daring players/i,
            ),
        ).toBeInTheDocument();
        expect(
            screen.getByText(
                /Join the Anarchy Win-Streak Challenge and try to get the longest running win streak/i,
            ),
        ).toBeInTheDocument();
    });

    it("should render the thumbnail image", () => {
        render(<WinStreakHeader />);

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
