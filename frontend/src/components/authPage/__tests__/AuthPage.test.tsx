import { render, screen } from "@testing-library/react";

import AuthPage from "../AuthPage";

describe("AuthPage", () => {
    it("should rendersthe logo text image", () => {
        render(<AuthPage />);
        const logoImg = screen.getByAltText(/logo/i);
        expect(logoImg).toBeInTheDocument();
    });

    it("should render Google OAuth button", () => {
        render(<AuthPage />);
        expect(screen.getByText(/Continue with Google/i)).toBeInTheDocument();
        expect(screen.getByAltText(/Google Icon/i)).toBeInTheDocument();
    });

    it("should render Discord OAuth button", () => {
        render(<AuthPage />);
        expect(screen.getByText(/Continue with Discord/i)).toBeInTheDocument();
        expect(screen.getByAltText(/Discord Icon/i)).toBeInTheDocument();
    });
});
